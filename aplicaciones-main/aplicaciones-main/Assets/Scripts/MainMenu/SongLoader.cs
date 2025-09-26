using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SongLoader : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;
    public GameObject songButtonPrefab;
    public GameObject loadingIndicator;
    
    [Header("Song Selection")]
    public Color selectedColor = Color.green;
    public Color defaultColor = Color.white;
    
    private List<SongData> availableSongs = new List<SongData>();
    private GameObject currentSelectedButton;

    void Start()
    {
        LoadSongs();
    }

    void LoadSongs()
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);
            
        string songsPath = Path.Combine(Application.streamingAssetsPath, "Songs");

        if (!Directory.Exists(songsPath))
        {
            Debug.LogError("❌ Carpeta 'Songs' no encontrada en StreamingAssets.");
            CreateNoSongsMessage();
            return;
        }

        // Clear previous content
        ClearSongList();
        availableSongs.Clear();

        string[] songFolders = Directory.GetDirectories(songsPath);

        if (songFolders.Length == 0)
        {
            CreateNoSongsMessage();
            return;
        }

        foreach (string folder in songFolders)
        {
            SongData songData = LoadSongData(folder);
            if (songData != null)
            {
                availableSongs.Add(songData);
                CreateSongButton(songData);
            }
        }
        
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
            
        Debug.Log($"🎵 Loaded {availableSongs.Count} songs");
    }
    
    void ClearSongList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        currentSelectedButton = null;
    }
    
    SongData LoadSongData(string folderPath)
    {
        string songName = Path.GetFileName(folderPath);
        string artist = "Unknown Artist";
        
        // Try to load song.ini for metadata
        string iniPath = Path.Combine(folderPath, "song.ini");
        if (File.Exists(iniPath))
        {
            try
            {
                string iniContent = File.ReadAllText(iniPath);
                
                // Extract song name
                Match nameMatch = Regex.Match(iniContent, @"name\s*=\s*(.+)", RegexOptions.IgnoreCase);
                if (nameMatch.Success)
                    songName = nameMatch.Groups[1].Value.Trim();
                
                // Extract artist
                Match artistMatch = Regex.Match(iniContent, @"artist\s*=\s*(.+)", RegexOptions.IgnoreCase);
                if (artistMatch.Success)
                    artist = artistMatch.Groups[1].Value.Trim();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Error reading song.ini for {songName}: {e.Message}");
            }
        }
        
        // Check for required files
        string oggPath = Path.Combine(folderPath, "song.ogg");
        string chartPath = Path.Combine(folderPath, "notes.chart");
        
        if (!File.Exists(oggPath))
        {
            Debug.LogWarning($"⚠️ Missing song.ogg for {songName}");
            return null;
        }
        
        if (!File.Exists(chartPath))
        {
            Debug.LogWarning($"⚠️ Missing notes.chart for {songName}");
            return null;
        }
        
        return new SongData(songName, artist, oggPath, chartPath, iniPath);
    }
    
    void CreateSongButton(SongData songData)
    {
        GameObject buttonObj = Instantiate(songButtonPrefab, contentParent);
        buttonObj.transform.localScale = Vector3.one;

        // Set up text components
        TextMeshProUGUI[] textComponents = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();
        
        if (textComponents.Length >= 1)
        {
            textComponents[0].text = songData.songName;
        }
        
        if (textComponents.Length >= 2)
        {
            textComponents[1].text = $"by {songData.artist}";
        }
        
        // Set up button functionality
        Button btn = buttonObj.GetComponent<Button>();
        if (btn != null)
        {
            // Store song data reference
            SongButtonData buttonData = buttonObj.AddComponent<SongButtonData>();
            buttonData.songData = songData;
            
            btn.onClick.AddListener(() => {
                SelectSong(songData, buttonObj);
            });
        }
        else
        {
            Debug.LogWarning("⚠️ El prefab no tiene componente Button.");
        }
    }
    
    void SelectSong(SongData songData, GameObject buttonObj)
    {
        // Update visual selection
        UpdateButtonSelection(buttonObj);
        
        // Update game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.selectedSongPath = Path.GetFileName(Path.GetDirectoryName(songData.oggPath));
            GameManager.Instance.UpdatePlayButtonState();
        }
        
        Debug.Log($"🎵 Canción seleccionada: {songData.songName} by {songData.artist}");
    }
    
    void UpdateButtonSelection(GameObject selectedButton)
    {
        // Reset previous selection
        if (currentSelectedButton != null)
        {
            Image prevImage = currentSelectedButton.GetComponent<Image>();
            if (prevImage != null)
                prevImage.color = defaultColor;
        }
        
        // Set new selection
        currentSelectedButton = selectedButton;
        Image newImage = selectedButton.GetComponent<Image>();
        if (newImage != null)
            newImage.color = selectedColor;
    }
    
    void CreateNoSongsMessage()
    {
        GameObject messageObj = new GameObject("NoSongsMessage");
        messageObj.transform.SetParent(contentParent, false);
        
        TextMeshProUGUI text = messageObj.AddComponent<TextMeshProUGUI>();
        text.text = "No songs found!\nPlace song folders in StreamingAssets/Songs/";
        text.fontSize = 18;
        text.color = Color.gray;
        text.alignment = TextAlignmentOptions.Center;
        
        RectTransform rect = text.rectTransform;
        rect.sizeDelta = new Vector2(400, 100);
        
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
    }
    
    public void RefreshSongList()
    {
        LoadSongs();
    }
    
    public SongData GetSelectedSong()
    {
        if (currentSelectedButton != null)
        {
            SongButtonData buttonData = currentSelectedButton.GetComponent<SongButtonData>();
            return buttonData?.songData;
        }
        return null;
    }
}

// Helper component to store song data reference in button
public class SongButtonData : MonoBehaviour
{
    public SongData songData;
}
