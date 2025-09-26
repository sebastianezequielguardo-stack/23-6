using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Gameplay UI")]
    public Text scoreText;
    public Text accuracyText;
    public Text comboText;
    public Text multiplierText;
    public Text songNameText;
    public Text timeText;
    public Slider progressSlider;
    
    [Header("Hit Feedback")]
    public Text hitFeedbackText;
    public GameObject[] laneEffects; // Visual effects for each lane
    public Color perfectColor = Color.green;
    public Color greatColor = Color.yellow;
    public Color goodColor = Color.orange;
    public Color missColor = Color.red;
    
    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;
    
    [Header("End Game UI")]
    public GameObject endGamePanel;
    public Text finalScoreText;
    public Text finalAccuracyText;
    public Text finalComboText;
    public Text perfectHitsText;
    public Text greatHitsText;
    public Text goodHitsText;
    public Text missedNotesText;
    public Button playAgainButton;
    public Button backToMenuButton;
    
    [Header("Loading UI")]
    public GameObject loadingPanel;
    public Text loadingText;
    public Slider loadingSlider;
    
    private GameplayManager gameplayManager;
    private ScoreManager scoreManager;
    private Coroutine hitFeedbackCoroutine;

    void Start()
    {
        InitializeUI();
        SetupEventListeners();
    }
    
    void InitializeUI()
    {
        gameplayManager = GameplayManager.Instance;
        scoreManager = FindObjectOfType<ScoreManager>();
        
        // Hide pause and end game menus initially
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (endGamePanel != null) endGamePanel.SetActive(false);
        
        // Set up song info
        if (songNameText != null && GameManager.Instance != null)
        {
            songNameText.text = GameManager.Instance.selectedSongPath;
        }
        
        // Initialize progress slider
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
        }
    }
    
    void SetupEventListeners()
    {
        // Subscribe to gameplay events
        if (gameplayManager != null)
        {
            gameplayManager.OnNoteHit += OnNoteHit;
            gameplayManager.OnNoteMissed += OnNoteMissed;
            gameplayManager.OnSongFinished += OnSongFinished;
        }
        
        // Subscribe to score events
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged += UpdateScore;
            scoreManager.OnComboChanged += UpdateCombo;
            scoreManager.OnMultiplierChanged += UpdateMultiplier;
        }
        
        // Setup button listeners
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
            
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(RestartGame);
            
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(GoToMainMenu);
    }

    void Update()
    {
        UpdateGameplayUI();
        HandlePauseInput();
    }
    
    void UpdateGameplayUI()
    {
        if (gameplayManager == null) return;
        
        // Update time and progress
        float currentTime = gameplayManager.GetSongTime();
        float songLength = gameplayManager.songLength;
        
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }
        
        if (progressSlider != null && songLength > 0)
        {
            progressSlider.value = currentTime / songLength;
        }
        
        // Update accuracy
        if (accuracyText != null && scoreManager != null)
        {
            accuracyText.text = $"Accuracy: {scoreManager.GetAccuracy():F1}%";
        }
    }
    
    void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameplayManager != null)
            {
                if (gameplayManager.isPaused)
                    ResumeGame();
                else
                    PauseGame();
            }
        }
    }
    
    void OnNoteHit(NoteData noteData, HitAccuracy accuracy)
    {
        // Show hit feedback
        ShowHitFeedback(accuracy);
        
        // Trigger lane effect
        TriggerLaneEffect(noteData.laneIndex, true);
    }
    
    void OnNoteMissed(NoteData noteData)
    {
        // Show miss feedback
        ShowHitFeedback(HitAccuracy.Miss);
    }
    
    void ShowHitFeedback(HitAccuracy accuracy)
    {
        if (hitFeedbackText == null) return;
        
        // Stop previous feedback coroutine
        if (hitFeedbackCoroutine != null)
            StopCoroutine(hitFeedbackCoroutine);
        
        // Set text and color based on accuracy
        switch (accuracy)
        {
            case HitAccuracy.Perfect:
                hitFeedbackText.text = "PERFECT!";
                hitFeedbackText.color = perfectColor;
                break;
            case HitAccuracy.Great:
                hitFeedbackText.text = "GREAT!";
                hitFeedbackText.color = greatColor;
                break;
            case HitAccuracy.Good:
                hitFeedbackText.text = "GOOD";
                hitFeedbackText.color = goodColor;
                break;
            case HitAccuracy.Miss:
                hitFeedbackText.text = "MISS";
                hitFeedbackText.color = missColor;
                break;
        }
        
        // Start fade out coroutine
        hitFeedbackCoroutine = StartCoroutine(FadeOutHitFeedback());
    }
    
    IEnumerator FadeOutHitFeedback()
    {
        if (hitFeedbackText == null) yield break;
        
        hitFeedbackText.gameObject.SetActive(true);
        
        Color originalColor = hitFeedbackText.color;
        float duration = 1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            hitFeedbackText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        hitFeedbackText.gameObject.SetActive(false);
    }
    
    void TriggerLaneEffect(int laneIndex, bool activate)
    {
        if (laneEffects != null && laneIndex >= 0 && laneIndex < laneEffects.Length)
        {
            if (laneEffects[laneIndex] != null)
            {
                laneEffects[laneIndex].SetActive(activate);
                
                if (activate)
                {
                    // Auto-deactivate after a short time
                    StartCoroutine(DeactivateLaneEffect(laneIndex, 0.2f));
                }
            }
        }
    }
    
    IEnumerator DeactivateLaneEffect(int laneIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (laneEffects != null && laneIndex >= 0 && laneIndex < laneEffects.Length)
        {
            if (laneEffects[laneIndex] != null)
            {
                laneEffects[laneIndex].SetActive(false);
            }
        }
    }
    
    void UpdateScore(int newScore)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {newScore:N0}";
    }
    
    void UpdateCombo(int newCombo)
    {
        if (comboText != null)
        {
            if (newCombo > 0)
                comboText.text = $"Combo: {newCombo}";
            else
                comboText.text = "";
        }
    }
    
    void UpdateMultiplier(int newMultiplier)
    {
        if (multiplierText != null)
            multiplierText.text = $"x{newMultiplier}";
    }
    
    void PauseGame()
    {
        if (gameplayManager != null)
        {
            gameplayManager.PauseGame();
            
            if (pauseMenu != null)
                pauseMenu.SetActive(true);
        }
    }
    
    void ResumeGame()
    {
        if (gameplayManager != null)
        {
            gameplayManager.ResumeGame();
            
            if (pauseMenu != null)
                pauseMenu.SetActive(false);
        }
    }
    
    void RestartGame()
    {
        Time.timeScale = 1f; // Reset time scale
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    void GoToMainMenu()
    {
        Time.timeScale = 1f; // Reset time scale
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    void OnSongFinished()
    {
        ShowEndGameScreen();
    }
    
    void ShowEndGameScreen()
    {
        if (endGamePanel == null || scoreManager == null) return;
        
        endGamePanel.SetActive(true);
        
        ScoreData finalScore = scoreManager.GetFinalScore();
        
        if (finalScoreText != null)
            finalScoreText.text = $"Final Score: {finalScore.finalScore:N0}";
            
        if (finalAccuracyText != null)
            finalAccuracyText.text = $"Accuracy: {finalScore.accuracy:F1}%";
            
        if (finalComboText != null)
            finalComboText.text = $"Max Combo: {finalScore.maxCombo}";
            
        if (perfectHitsText != null)
            perfectHitsText.text = $"Perfect: {finalScore.perfectHits}";
            
        if (greatHitsText != null)
            greatHitsText.text = $"Great: {finalScore.greatHits}";
            
        if (goodHitsText != null)
            goodHitsText.text = $"Good: {finalScore.goodHits}";
            
        if (missedNotesText != null)
            missedNotesText.text = $"Missed: {finalScore.missedNotes}";
    }
    
    public void ShowLoadingScreen(bool show)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(show);
    }
    
    public void UpdateLoadingProgress(float progress, string text = "Loading...")
    {
        if (loadingSlider != null)
            loadingSlider.value = progress;
            
        if (loadingText != null)
            loadingText.text = text;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (gameplayManager != null)
        {
            gameplayManager.OnNoteHit -= OnNoteHit;
            gameplayManager.OnNoteMissed -= OnNoteMissed;
            gameplayManager.OnSongFinished -= OnSongFinished;
        }
        
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged -= UpdateScore;
            scoreManager.OnComboChanged -= UpdateCombo;
            scoreManager.OnMultiplierChanged -= UpdateMultiplier;
        }
    }
}
