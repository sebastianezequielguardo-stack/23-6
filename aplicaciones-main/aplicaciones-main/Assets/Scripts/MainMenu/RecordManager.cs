using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class RecordManager : MonoBehaviour
{
    public Text songNameText;
    public Text[] recordTexts; // Asigná 5 Texts en el Inspector

    void HandleSongSelected(string path)
    {
        string songName = Path.GetFileName(path);
        songNameText.text = "Records de: " + songName;

        string recordFile = Path.Combine(path, "records.txt");

        if (!File.Exists(recordFile))
        {
            for (int i = 0; i < recordTexts.Length; i++)
                recordTexts[i].text = $"{i + 1}. ---";
            return;
        }

        string[] lines = File.ReadAllLines(recordFile);
        for (int i = 0; i < recordTexts.Length; i++)
        {
            if (i < lines.Length)
                recordTexts[i].text = $"{i + 1}. {lines[i]}";
            else
                recordTexts[i].text = $"{i + 1}. ---";
        }
    }
}
