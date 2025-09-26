using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChartParser : MonoBehaviour
{
    public string chartPath;
    public string difficultySection = "EasySingle"; // Cambiar dinámicamente

    public List<string> noteLines = new List<string>();

    public void LoadChart()
    {
        noteLines.Clear();

        if (!File.Exists(chartPath)) return;

        string[] lines = File.ReadAllLines(chartPath);
        bool inSection = false;

        foreach (string line in lines)
        {
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                inSection = line.Contains(difficultySection);
                continue;
            }

            if (inSection)
            {
                if (line.StartsWith("{") || line.StartsWith("}")) continue;
                noteLines.Add(line);
            }
        }

        Debug.Log($"Notas cargadas para {difficultySection}: {noteLines.Count}");
    }
}
