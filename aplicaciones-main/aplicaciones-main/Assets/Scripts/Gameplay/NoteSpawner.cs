using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
    public Transform[] lanes;
    public float noteSpeed = 5f;
    
    [Header("Spawn Settings")]
    public float spawnDistance = 10f; // Distance ahead of hit zone to spawn notes
    public float hitZoneY = 0f; // Y position of the hit zone
    
    private List<NoteData> notes = new List<NoteData>();
    private float bpm = 120f;
    private float resolution = 192f;
    private float offset = 0f;
    
    // Events
    public System.Action<NoteData, Note> OnNoteSpawned;

    IEnumerator Start()
    {
        while (string.IsNullOrEmpty(GameplayManager.Instance.selectedChartSection))
            yield return null;

        ExtractChartTiming();
        ParseChart();
    }

    void Update()
    {
        float songTime = GameplayManager.Instance.GetSongTime();

        foreach (NoteData note in notes)
        {
            if (!note.spawned && songTime >= note.time)
            {
                SpawnNote(note);
                note.spawned = true;
            }
        }
    }

    void ExtractChartTiming()
    {
        string chart = GameplayManager.Instance.chartData;

        Match resMatch = Regex.Match(chart, @"Resolution\s*=\s*(\d+)");
        if (resMatch.Success)
            resolution = float.Parse(resMatch.Groups[1].Value);

        Match bpmMatch = Regex.Match(chart, @"B\s+(\d+)");
        if (bpmMatch.Success)
            bpm = float.Parse(bpmMatch.Groups[1].Value) / 1000f;

        Match offsetMatch = Regex.Match(chart, @"Offset\s*=\s*(-?\d+(\.\d+)?)");
        if (offsetMatch.Success)
            offset = float.Parse(offsetMatch.Groups[1].Value);
    }

    void ParseChart()
    {
        string[] lines = GameplayManager.Instance.selectedChartSection.Split('\n');

        foreach (string line in lines)
        {
            if (line.Contains(" = N "))
            {
                string[] parts = line.Split(new[] { " = N " }, System.StringSplitOptions.None);
                string[] noteParts = parts[1].Split(' ');

                if (int.TryParse(parts[0].Trim(), out int tick) &&
                    int.TryParse(noteParts[0], out int laneIndex))
                {
                    if (laneIndex >= 0 && laneIndex <= 4)
                    {
                        float time = TickToSeconds(tick);
                        NoteData newNote = new NoteData(time, laneIndex);
                        newNote.tick = tick;
                        notes.Add(newNote);
                    }
                }
            }
        }
    }

    void SpawnNote(NoteData noteData)
    {
        if (noteData.laneIndex >= 0 && noteData.laneIndex < lanes.Length)
        {
            // Calculate spawn position (above the hit zone)
            Vector3 lanePosition = lanes[noteData.laneIndex].position;
            Vector3 spawnPosition = new Vector3(lanePosition.x, hitZoneY + spawnDistance, lanePosition.z);
            
            GameObject newNoteObject = Instantiate(notePrefab, spawnPosition, Quaternion.identity);

            Note noteScript = newNoteObject.GetComponent<Note>();
            if (noteScript != null)
            {
                // Set up the note script
                noteScript.lane = noteData.laneIndex;
                noteScript.speed = noteSpeed;
                noteScript.noteData = noteData;
                
                // Set up visual properties based on note data
                SpriteRenderer sr = newNoteObject.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = noteData.noteColor;
                }
                
                // Subscribe to note destruction event
                noteScript.OnNoteDestroyed += OnNoteObjectDestroyed;
            }
            
            // Notify that a note has been spawned
            OnNoteSpawned?.Invoke(noteData, noteScript);

            Debug.Log($"🎯 Nota instanciada en lane {noteData.laneIndex} - Tiempo: {noteData.time:F2}");
        }
    }
    
    void OnNoteObjectDestroyed(Note note)
    {
        // Clean up when a note is destroyed
        if (note != null)
        {
            note.OnNoteDestroyed -= OnNoteObjectDestroyed;
        }
    }

    float TickToSeconds(int tick)
    {
        return ((tick / resolution) * (60f / bpm)) + offset;
    }

}
