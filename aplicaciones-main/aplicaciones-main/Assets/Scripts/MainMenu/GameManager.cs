using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string selectedSongPath;
    public string selectedDifficulty = "Normal";
    public string selectedChartSection = "Full";

    public bool songSelected => !string.IsNullOrEmpty(selectedSongPath);
    public bool difficultySelected => !string.IsNullOrEmpty(selectedDifficulty);

    public System.Action OnPlayButtonStateChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Asegurate de que este objeto esté en la raíz
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdatePlayButtonState()
    {
        OnPlayButtonStateChanged?.Invoke();
    }
}
