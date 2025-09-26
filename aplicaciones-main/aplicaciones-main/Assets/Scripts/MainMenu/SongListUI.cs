using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SongListUI : MonoBehaviour
{
    [Header("Prefabs y UI")]
    public GameObject songButtonPrefab;
    public Transform contentPanel;

    public static string selectedSongPath = "";

    void Start()
    {
        string songsFolderPath = Path.Combine(Application.streamingAssetsPath, "Songs");

        if (!Directory.Exists(songsFolderPath))
        {
            Debug.LogWarning("No se encontró la carpeta Songs.");
            return;
        }

        // 🔄 Limpiar botones previos (incluso en modo edición)
        foreach (Transform child in contentPanel)
        {
            DestroyImmediate(child.gameObject);
        }

        string[] songFolders = Directory.GetDirectories(songsFolderPath);
        Debug.Log("Cantidad de carpetas encontradas: " + songFolders.Length);

        foreach (string folder in songFolders)
        {
            string folderName = Path.GetFileName(folder);
            Debug.Log("📁 Carpeta detectada: " + folderName);

            // Ignorar carpetas basura
            if (folderName.StartsWith(".") || folderName == "__MACOSX")
            {
                Debug.Log("Ignorando carpeta basura: " + folderName);
                continue;
            }

            string iniPath = Path.Combine(folder, "song.ini");
            string chartPath = Path.Combine(folder, "notes.chart");

            // Verificar que tenga song.ini y notes.chart
            if (!File.Exists(iniPath) || !File.Exists(chartPath))
            {
                Debug.Log("Carpeta ignorada (incompleta): " + folderName);
                continue;
            }

            // Leer nombre de la canción
            string songName = "Canción desconocida";
            string[] lines = File.ReadAllLines(iniPath);
            foreach (string line in lines)
            {
                if (line.StartsWith("name"))
                {
                    songName = line.Split('=')[1].Trim();
                    break;
                }
            }

            // Instanciar botón
            GameObject button = Instantiate(songButtonPrefab, contentPanel);

            // Forzar altura si el layout no lo controla
            RectTransform rt = button.GetComponent<RectTransform>();
            if (rt != null)
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, 80);

            // Asignar texto
            Text textComponent = button.GetComponentInChildren<Text>();
            if (textComponent != null)
                textComponent.text = songName;

            // Asignar funcionalidad
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedSongPath = folder;
                Debug.Log("🎵 Canción seleccionada: " + songName);
            });
        }
    }
}
