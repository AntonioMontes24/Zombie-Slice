using System.IO;
using TMPro;
using UnityEngine;

public class TextReaderController : MonoBehaviour
{
    public TMP_Text narrationText;
    public string filename;  //Intro file name
    // public string filename = "StoryIntro";  //Intro file name
    private string[] lines;
    private int currentLine = -1;//Default first line of text file

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename + ".txt");
        if (File.Exists(path))
        {
            lines = File.ReadAllLines(path);
            narrationText.text = "";
        }
        else
        {
            Debug.LogError("Story file not found: " + path);
        }
        
    }

    public void ShowNextLine()
    {
        if (lines == null) return;

        currentLine++;
        if (currentLine < lines.Length)
        {
            narrationText.text = lines[currentLine];
            Debug.Log("Showing line: " + currentLine);
        }
    }

}
