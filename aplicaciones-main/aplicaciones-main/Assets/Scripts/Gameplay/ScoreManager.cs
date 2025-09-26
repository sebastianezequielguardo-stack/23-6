using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Values")]
    public int perfectScore = 100;
    public int greatScore = 75;
    public int goodScore = 50;
    public int missScore = -25;
    
    [Header("Multiplier Settings")]
    public int maxMultiplier = 4;
    public int notesForMultiplier = 10; // Notes needed to increase multiplier
    
    [Header("UI References")]
    public Text scoreText;
    public Text accuracyText;
    public Text multiplierText;
    public Text comboText;
    
    // Score tracking
    public int score = 0;
    public int totalNotes = 0;
    public int hitNotes = 0;
    public int perfectHits = 0;
    public int greatHits = 0;
    public int goodHits = 0;
    public int missedNotes = 0;
    
    // Combo and multiplier
    public int currentCombo = 0;
    public int maxCombo = 0;
    public int currentMultiplier = 1;
    public int consecutiveHits = 0;
    
    // Events
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnComboChanged;
    public System.Action<int> OnMultiplierChanged;

    void Start()
    {
        UpdateUI();
        
        // Subscribe to gameplay events
        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.OnNoteHit += OnNoteHit;
            GameplayManager.Instance.OnNoteMissed += OnNoteMiss;
        }
    }

    public void RegisterHit(HitAccuracy accuracy)
    {
        totalNotes++;
        hitNotes++;
        consecutiveHits++;
        currentCombo++;
        
        // Track hit types
        switch (accuracy)
        {
            case HitAccuracy.Perfect:
                perfectHits++;
                score += perfectScore * currentMultiplier;
                break;
            case HitAccuracy.Great:
                greatHits++;
                score += greatScore * currentMultiplier;
                break;
            case HitAccuracy.Good:
                goodHits++;
                score += goodScore * currentMultiplier;
                break;
        }
        
        // Update max combo
        if (currentCombo > maxCombo)
            maxCombo = currentCombo;
        
        // Update multiplier
        UpdateMultiplier();
        
        // Trigger events
        OnScoreChanged?.Invoke(score);
        OnComboChanged?.Invoke(currentCombo);
        
        UpdateUI();
        
        Debug.Log($"🎯 Hit registered - Accuracy: {accuracy}, Score: +{GetScoreForAccuracy(accuracy) * currentMultiplier}, Combo: {currentCombo}");
    }

    public void RegisterMiss()
    {
        totalNotes++;
        missedNotes++;
        consecutiveHits = 0;
        currentCombo = 0;
        currentMultiplier = 1;
        
        score += missScore; // Negative score for misses
        if (score < 0) score = 0; // Don't go below 0
        
        // Trigger events
        OnScoreChanged?.Invoke(score);
        OnComboChanged?.Invoke(currentCombo);
        OnMultiplierChanged?.Invoke(currentMultiplier);
        
        UpdateUI();
        
        Debug.Log($"❌ Miss registered - Score: {missScore}, Combo broken");
    }
    
    void UpdateMultiplier()
    {
        int newMultiplier = Mathf.Min(1 + (consecutiveHits / notesForMultiplier), maxMultiplier);
        
        if (newMultiplier != currentMultiplier)
        {
            currentMultiplier = newMultiplier;
            OnMultiplierChanged?.Invoke(currentMultiplier);
            Debug.Log($"🔥 Multiplier increased to {currentMultiplier}x!");
        }
    }
    
    int GetScoreForAccuracy(HitAccuracy accuracy)
    {
        switch (accuracy)
        {
            case HitAccuracy.Perfect: return perfectScore;
            case HitAccuracy.Great: return greatScore;
            case HitAccuracy.Good: return goodScore;
            default: return 0;
        }
    }
    
    void OnNoteHit(NoteData noteData, HitAccuracy accuracy)
    {
        // Additional processing when a note is hit
        // This is called from GameplayManager events
    }
    
    void OnNoteMiss(NoteData noteData)
    {
        // Additional processing when a note is missed
        // This is called from GameplayManager events
    }

    public float GetAccuracy()
    {
        return totalNotes > 0 ? (hitNotes / (float)totalNotes) * 100f : 0f;
    }
    
    public float GetPerfectPercentage()
    {
        return totalNotes > 0 ? (perfectHits / (float)totalNotes) * 100f : 0f;
    }
    
    public ScoreData GetFinalScore()
    {
        return new ScoreData
        {
            finalScore = score,
            accuracy = GetAccuracy(),
            maxCombo = maxCombo,
            perfectHits = perfectHits,
            greatHits = greatHits,
            goodHits = goodHits,
            missedNotes = missedNotes,
            totalNotes = totalNotes
        };
    }
    
    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score:N0}";
            
        if (accuracyText != null)
            accuracyText.text = $"Accuracy: {GetAccuracy():F1}%";
            
        if (multiplierText != null)
            multiplierText.text = $"x{currentMultiplier}";
            
        if (comboText != null)
            comboText.text = $"Combo: {currentCombo}";
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.OnNoteHit -= OnNoteHit;
            GameplayManager.Instance.OnNoteMissed -= OnNoteMiss;
        }
    }
}

[System.Serializable]
public class ScoreData
{
    public int finalScore;
    public float accuracy;
    public int maxCombo;
    public int perfectHits;
    public int greatHits;
    public int goodHits;
    public int missedNotes;
    public int totalNotes;
}
