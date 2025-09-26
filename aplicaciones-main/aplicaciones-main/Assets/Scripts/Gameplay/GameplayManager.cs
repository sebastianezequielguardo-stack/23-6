using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;

    [Header("Audio")]
    public AudioSource musicSource;

    [Header("Spawner")]
    public NoteSpawner spawner;
    
    [Header("Managers")]
    public ScoreManager scoreManager;
    public InputManager inputManager;
    
    [Header("Game State")]
    public bool isGameActive = false;
    public bool isPaused = false;
    public float songLength = 0f;
    
    [Header("Hit Detection")]
    public float hitWindow = 0.1f;
    public float perfectWindow = 0.05f;
    public float greatWindow = 0.08f;
    
    public string chartData;
    public string selectedChartSection;
    
    // Events
    public System.Action<NoteData, HitAccuracy> OnNoteHit;
    public System.Action<NoteData> OnNoteMissed;
    public System.Action OnSongFinished;
    
    // Active notes for hit detection
    private List<NoteData> activeNotes = new List<NoteData>();
    private Dictionary<int, Note> spawnedNotes = new Dictionary<int, Note>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(InitializeGameplay());
    }
    
    IEnumerator InitializeGameplay()
    {
        LoadChartSection();
        yield return StartCoroutine(LoadAudio());
        
        // Wait a moment for everything to initialize
        yield return new WaitForSeconds(0.5f);
        
        StartGameplay();
    }
    
    void StartGameplay()
    {
        isGameActive = true;
        if (musicSource.clip != null)
        {
            songLength = musicSource.clip.length;
        }
        
        // Subscribe to events
        if (spawner != null)
        {
            spawner.OnNoteSpawned += RegisterActiveNote;
        }
    }

    IEnumerator LoadAudio()
    {
        string songFolder = Path.Combine(Application.streamingAssetsPath, "Songs", GameManager.Instance.selectedSongPath);
        string audioPath = Path.Combine(songFolder, "song.ogg");

        if (!File.Exists(audioPath))
        {
            Debug.LogError("❌ Audio no encontrado: " + audioPath);
            yield break;
        }

        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + audioPath, AudioType.OGGVORBIS);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Error al cargar audio: " + www.error);
        }
        else
        {
            musicSource.clip = DownloadHandlerAudioClip.GetContent(www);
            musicSource.volume = 0.4f;
            musicSource.Play();
        }
    }

    void LoadChartSection()
    {
        string songFolder = Path.Combine(Application.streamingAssetsPath, "Songs", GameManager.Instance.selectedSongPath);
        string chartPath = Path.Combine(songFolder, "notes.chart");

        if (!File.Exists(chartPath))
        {
            Debug.LogError("❌ Chart no encontrado: " + chartPath);
            return;
        }

        chartData = File.ReadAllText(chartPath);

        string[] possibleTags = GameManager.Instance.selectedDifficulty == "Facil"
            ? new[] { "[EasySingle]", "[EasyGuitar]", "[Single]" }
            : new[] { "[HardSingle]", "[HardGuitar]", "[ExpertSingle]", "[ExpertGuitar]" };

        foreach (string tag in possibleTags)
        {
            int startIndex = chartData.IndexOf(tag);
            if (startIndex != -1)
            {
                int endIndex = chartData.IndexOf('[', startIndex + tag.Length);
                if (endIndex == -1) endIndex = chartData.Length;

                selectedChartSection = chartData.Substring(startIndex, endIndex - startIndex);
                Debug.Log("🎯 Sección de dificultad encontrada: " + tag);
                return;
            }
        }

        Debug.LogError("❌ No se encontró ninguna sección válida para la dificultad seleccionada.");
    }

    public float GetSongTime()
    {
        if (musicSource != null && musicSource.clip != null && musicSource.isPlaying)
            return musicSource.time;
        else
            return 0f;
    }
    
    void Update()
    {
        if (!isGameActive || isPaused) return;
        
        // Check for missed notes
        CheckMissedNotes();
        
        // Check if song is finished
        if (musicSource != null && !musicSource.isPlaying && GetSongTime() > 0)
        {
            EndSong();
        }
    }
    
    void CheckMissedNotes()
    {
        float currentTime = GetSongTime();
        
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            NoteData note = activeNotes[i];
            
            if (note.ShouldBeMissed(currentTime, hitWindow + 0.05f))
            {
                note.MarkAsMissed();
                OnNoteMissed?.Invoke(note);
                scoreManager?.RegisterMiss();
                activeNotes.RemoveAt(i);
                
                Debug.Log($"❌ Nota perdida en lane {note.laneIndex} - Tiempo: {currentTime:F2}");
            }
        }
    }
    
    public void RegisterActiveNote(NoteData noteData, Note noteObject)
    {
        if (!activeNotes.Contains(noteData))
        {
            activeNotes.Add(noteData);
            spawnedNotes[noteData.GetHashCode()] = noteObject;
        }
    }
    
    public bool TryHitNote(int laneIndex, out HitAccuracy accuracy)
    {
        accuracy = HitAccuracy.Miss;
        float currentTime = GetSongTime();
        
        // Find the closest hittable note in the specified lane
        NoteData closestNote = null;
        float closestDistance = float.MaxValue;
        
        foreach (NoteData note in activeNotes)
        {
            if (note.laneIndex == laneIndex && note.CanBeHit(currentTime, hitWindow))
            {
                float distance = Mathf.Abs(currentTime - note.time);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNote = note;
                }
            }
        }
        
        if (closestNote != null)
        {
            // Determine accuracy based on timing
            if (closestDistance <= perfectWindow)
                accuracy = HitAccuracy.Perfect;
            else if (closestDistance <= greatWindow)
                accuracy = HitAccuracy.Great;
            else
                accuracy = HitAccuracy.Good;
            
            closestNote.MarkAsHit(accuracy);
            OnNoteHit?.Invoke(closestNote, accuracy);
            scoreManager?.RegisterHit(accuracy);
            
            // Remove from active notes
            activeNotes.Remove(closestNote);
            
            // Destroy the visual note
            if (spawnedNotes.TryGetValue(closestNote.GetHashCode(), out Note noteObject))
            {
                if (noteObject != null)
                    noteObject.Hit();
                spawnedNotes.Remove(closestNote.GetHashCode());
            }
            
            Debug.Log($"✅ Nota acertada en lane {laneIndex} - Precisión: {accuracy}");
            return true;
        }
        
        // No note to hit - register as a miss
        scoreManager?.RegisterMiss();
        Debug.Log($"❌ Fallo en lane {laneIndex} - Sin nota disponible");
        return false;
    }
    
    public void PauseGame()
    {
        isPaused = true;
        if (musicSource.isPlaying)
            musicSource.Pause();
        Time.timeScale = 0f;
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        if (musicSource.clip != null)
            musicSource.UnPause();
        Time.timeScale = 1f;
    }
    
    public void EndSong()
    {
        isGameActive = false;
        OnSongFinished?.Invoke();
        
        Debug.Log($"🎵 Canción terminada - Puntuación final: {scoreManager?.score}");
        
        // Return to main menu after a delay
        StartCoroutine(ReturnToMainMenu());
    }
    
    IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("MainMenu");
    }
    
    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnNoteSpawned -= RegisterActiveNote;
        }
    }
}
