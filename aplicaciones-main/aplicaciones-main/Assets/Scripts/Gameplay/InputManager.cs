using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("Input Configuration")]
    public KeyCode[] laneKeys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L };
    public KeyCode pauseKey = KeyCode.Escape;
    public KeyCode restartKey = KeyCode.R;
    
    [Header("Input Feedback")]
    public GameObject[] hitEffects; // Visual effects for each lane
    public AudioSource hitSound;
    public AudioSource missSound;
    
    private GameplayManager gameplayManager;
    private bool[] keyHeld = new bool[5]; // Track held keys for sustained notes

    void Start()
    {
        gameplayManager = GameplayManager.Instance;
        
        // Initialize key held array
        keyHeld = new bool[laneKeys.Length];
        
        // Subscribe to events for feedback
        if (gameplayManager != null)
        {
            gameplayManager.OnNoteHit += OnNoteHitFeedback;
            gameplayManager.OnNoteMissed += OnNoteMissedFeedback;
        }
    }

    void Update()
    {
        if (gameplayManager == null || !gameplayManager.isGameActive)
            return;
            
        HandleGameplayInput();
        HandleSystemInput();
    }
    
    void HandleGameplayInput()
    {
        for (int i = 0; i < laneKeys.Length; i++)
        {
            // Key press detection
            if (Input.GetKeyDown(laneKeys[i]))
            {
                keyHeld[i] = true;
                TryHitNote(i);
                TriggerLaneEffect(i, true);
            }
            
            // Key release detection (for sustained notes)
            if (Input.GetKeyUp(laneKeys[i]))
            {
                keyHeld[i] = false;
                TriggerLaneEffect(i, false);
            }
        }
    }
    
    void HandleSystemInput()
    {
        // Pause/Resume
        if (Input.GetKeyDown(pauseKey))
        {
            if (gameplayManager.isPaused)
                gameplayManager.ResumeGame();
            else
                gameplayManager.PauseGame();
        }
        
        // Restart (only when paused)
        if (Input.GetKeyDown(restartKey) && gameplayManager.isPaused)
        {
            RestartSong();
        }
    }

    void TryHitNote(int laneIndex)
    {
        if (gameplayManager.TryHitNote(laneIndex, out HitAccuracy accuracy))
        {
            // Note was hit successfully
            PlayHitSound(accuracy);
        }
        else
        {
            // No note to hit or missed
            PlayMissSound();
        }
    }
    
    void TriggerLaneEffect(int laneIndex, bool pressed)
    {
        if (hitEffects != null && laneIndex < hitEffects.Length && hitEffects[laneIndex] != null)
        {
            // Activate/deactivate visual effect for the lane
            hitEffects[laneIndex].SetActive(pressed);
        }
    }
    
    void PlayHitSound(HitAccuracy accuracy)
    {
        if (hitSound != null)
        {
            // Adjust pitch based on accuracy
            switch (accuracy)
            {
                case HitAccuracy.Perfect:
                    hitSound.pitch = 1.2f;
                    break;
                case HitAccuracy.Great:
                    hitSound.pitch = 1.1f;
                    break;
                case HitAccuracy.Good:
                    hitSound.pitch = 1.0f;
                    break;
            }
            
            hitSound.Play();
        }
    }
    
    void PlayMissSound()
    {
        if (missSound != null)
        {
            missSound.Play();
        }
    }
    
    void OnNoteHitFeedback(NoteData noteData, HitAccuracy accuracy)
    {
        // Additional feedback when a note is hit
        Debug.Log($"🎯 Hit feedback - Lane: {noteData.laneIndex}, Accuracy: {accuracy}");
    }
    
    void OnNoteMissedFeedback(NoteData noteData)
    {
        // Additional feedback when a note is missed
        Debug.Log($"💥 Miss feedback - Lane: {noteData.laneIndex}");
    }
    
    void RestartSong()
    {
        // Restart the current song
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    // Public method to check if a key is currently held (for sustained notes)
    public bool IsKeyHeld(int laneIndex)
    {
        return laneIndex >= 0 && laneIndex < keyHeld.Length && keyHeld[laneIndex];
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (gameplayManager != null)
        {
            gameplayManager.OnNoteHit -= OnNoteHitFeedback;
            gameplayManager.OnNoteMissed -= OnNoteMissedFeedback;
        }
    }
}
