using UnityEngine;

[System.Serializable]
public class SongData
{
    public string songName;
    public string artist;
    public string oggPath;
    public string chartPath;
    public string iniPath;
    public string rb3conPath;

    public SongData(string name, string artist, string ogg, string chart, string ini, string rb3con = "")
    {
        songName = name;
        this.artist = artist;
        oggPath = ogg;
        chartPath = chart;
        iniPath = ini;
        rb3conPath = rb3con;
    }
}
