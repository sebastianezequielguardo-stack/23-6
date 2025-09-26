using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DifficultyManager : MonoBehaviour
{
    [Header("Difficulty Buttons")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button expertButton;
    
    [Header("Visual Feedback")]
    public Color selectedColor = Color.green;
    public Color unselectedColor = Color.white;
    public Color disabledColor = Color.gray;
    
    [Header("Difficulty Info")]
    public TextMeshProUGUI difficultyInfoText;
    public GameObject difficultyInfoPanel;
    
    private Dictionary<string, Button> difficultyButtons = new Dictionary<string, Button>();
    private Dictionary<string, DifficultyInfo> difficultyInfos = new Dictionary<string, DifficultyInfo>();
    private string currentSelectedDifficulty = "";

    void Start()
    {
        InitializeDifficulties();
        SetupButtons();
        UpdateDifficultyInfo();
    }
    
    void InitializeDifficulties()
    {
        // Set up difficulty information
        difficultyInfos["Easy"] = new DifficultyInfo
        {
            name = "Easy",
            description = "Perfect for beginners. Slower note speed and simpler patterns.",
            noteSpeed = 3f,
            chartTags = new[] { "[EasySingle]", "[EasyGuitar]", "[Single]" }
        };
        
        difficultyInfos["Medium"] = new DifficultyInfo
        {
            name = "Medium", 
            description = "Moderate challenge with standard note speed.",
            noteSpeed = 5f,
            chartTags = new[] { "[MediumSingle]", "[MediumGuitar]", "[Single]" }
        };
        
        difficultyInfos["Hard"] = new DifficultyInfo
        {
            name = "Hard",
            description = "Fast-paced gameplay for experienced players.",
            noteSpeed = 7f,
            chartTags = new[] { "[HardSingle]", "[HardGuitar]", "[ExpertSingle]" }
        };
        
        difficultyInfos["Expert"] = new DifficultyInfo
        {
            name = "Expert",
            description = "Maximum challenge! Lightning-fast notes and complex patterns.",
            noteSpeed = 9f,
            chartTags = new[] { "[ExpertSingle]", "[ExpertGuitar]", "[HardSingle]" }
        };
        
        // Map buttons to difficulties
        if (easyButton != null) difficultyButtons["Easy"] = easyButton;
        if (mediumButton != null) difficultyButtons["Medium"] = mediumButton;
        if (hardButton != null) difficultyButtons["Hard"] = hardButton;
        if (expertButton != null) difficultyButtons["Expert"] = expertButton;
    }
    
    void SetupButtons()
    {
        foreach (var kvp in difficultyButtons)
        {
            string difficulty = kvp.Key;
            Button button = kvp.Value;
            
            if (button != null)
            {
                button.onClick.AddListener(() => SetDifficulty(difficulty));
                
                // Set initial color
                ColorBlock colors = button.colors;
                colors.normalColor = unselectedColor;
                colors.highlightedColor = Color.Lerp(unselectedColor, Color.white, 0.2f);
                button.colors = colors;
            }
        }
        
        // Set default difficulty
        if (difficultyButtons.ContainsKey("Medium"))
        {
            SetDifficulty("Medium");
        }
        else if (difficultyButtons.Count > 0)
        {
            SetDifficulty(difficultyButtons.Keys.GetEnumerator().Current);
        }
    }

    void SetDifficulty(string difficulty)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ [DifficultyManager] GameManager.Instance es null.");
            return;
        }
        
        // Update visual selection
        UpdateButtonVisuals(difficulty);
        
        // Update game manager
        currentSelectedDifficulty = difficulty;
        GameManager.Instance.selectedDifficulty = GetLegacyDifficultyName(difficulty);
        GameManager.Instance.UpdatePlayButtonState();
        
        // Update info display
        UpdateDifficultyInfo();
        
        Debug.Log($"🎯 Dificultad seleccionada: {difficulty}");
    }
    
    void UpdateButtonVisuals(string selectedDifficulty)
    {
        foreach (var kvp in difficultyButtons)
        {
            string difficulty = kvp.Key;
            Button button = kvp.Value;
            
            if (button == null) continue;
            
            ColorBlock colors = button.colors;
            
            if (difficulty == selectedDifficulty)
            {
                colors.normalColor = selectedColor;
                colors.highlightedColor = Color.Lerp(selectedColor, Color.white, 0.3f);
            }
            else
            {
                colors.normalColor = unselectedColor;
                colors.highlightedColor = Color.Lerp(unselectedColor, Color.white, 0.2f);
            }
            
            button.colors = colors;
        }
    }
    
    void UpdateDifficultyInfo()
    {
        if (difficultyInfoText == null || string.IsNullOrEmpty(currentSelectedDifficulty)) return;
        
        if (difficultyInfos.ContainsKey(currentSelectedDifficulty))
        {
            DifficultyInfo info = difficultyInfos[currentSelectedDifficulty];
            difficultyInfoText.text = $"<b>{info.name}</b>\n{info.description}\nNote Speed: {info.noteSpeed}";
        }
    }
    
    // Convert new difficulty names to legacy names for compatibility
    string GetLegacyDifficultyName(string newDifficulty)
    {
        switch (newDifficulty)
        {
            case "Easy": return "Facil";
            case "Medium": return "Normal";
            case "Hard": return "Dificil";
            case "Expert": return "Experto";
            default: return newDifficulty;
        }
    }
    
    // Public method to get current difficulty info
    public DifficultyInfo GetCurrentDifficultyInfo()
    {
        if (difficultyInfos.ContainsKey(currentSelectedDifficulty))
        {
            return difficultyInfos[currentSelectedDifficulty];
        }
        return null;
    }
    
    // Public method to check if a difficulty is available
    public bool IsDifficultyAvailable(string difficulty)
    {
        return difficultyButtons.ContainsKey(difficulty) && difficultyButtons[difficulty] != null;
    }
    
    // Public method to enable/disable specific difficulties
    public void SetDifficultyEnabled(string difficulty, bool enabled)
    {
        if (difficultyButtons.ContainsKey(difficulty) && difficultyButtons[difficulty] != null)
        {
            Button button = difficultyButtons[difficulty];
            button.interactable = enabled;
            
            ColorBlock colors = button.colors;
            colors.normalColor = enabled ? unselectedColor : disabledColor;
            button.colors = colors;
        }
    }
    
    // Show/hide difficulty info panel
    public void ShowDifficultyInfo(bool show)
    {
        if (difficultyInfoPanel != null)
        {
            difficultyInfoPanel.SetActive(show);
        }
    }
}

[System.Serializable]
public class DifficultyInfo
{
    public string name;
    public string description;
    public float noteSpeed;
    public string[] chartTags;
}
