using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Jugar()
    {
        if (string.IsNullOrEmpty(GameManager.Instance.selectedSongPath))
        {
            Debug.LogWarning("⚠️ No se seleccionó ninguna canción.");
            return;
        }

        SceneManager.LoadScene("Gameplay");
    }

    public void SetFacil()
    {
        GameManager.Instance.selectedDifficulty = "Facil";
        Debug.Log("🎯 Dificultad: Fácil");
    }

    public void SetDificil()
    {
        GameManager.Instance.selectedDifficulty = "Dificil";
        Debug.Log("🎯 Dificultad: Difícil");
    }
}
