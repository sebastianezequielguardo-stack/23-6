using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class RecordDisplay : MonoBehaviour
{
    public GameObject recordEntryPrefab;
    public Transform recordContentPanel;

    private List<RecordData> currentRecords = new List<RecordData>();

    public void LoadRecords(List<RecordData> newRecords)
    {
        // Limpiar los anteriores
        foreach (Transform child in recordContentPanel)
        {
            Destroy(child.gameObject);
        }

        // Ordenar por puntos (o porcentaje si preferís)
        currentRecords = newRecords.OrderByDescending(r => r.points).Take(5).ToList();

        for (int i = 0; i < currentRecords.Count; i++)
        {
            var record = currentRecords[i];
            GameObject entry = Instantiate(recordEntryPrefab, recordContentPanel);

            Text[] texts = entry.GetComponentsInChildren<Text>();
            texts[0].text = $"{i + 1}°";
            texts[1].text = $"{record.points} pts";
            texts[2].text = $"{record.accuracy}%";
        }
    }

    public void AddNewRecord(RecordData newRecord)
    {
        currentRecords.Add(newRecord);
        LoadRecords(currentRecords);
    }
}

[System.Serializable]
public class RecordData
{
    public int points;
    public float accuracy;
}
