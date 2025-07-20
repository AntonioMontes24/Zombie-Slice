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
    public GameObject creditContainer;
    public bool isCreditsMode = false; //Default false

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadTextFile(filename);  // Same as your original Start logic, now reusable.
    }

    public void LoadTextFile(string file)
    {
        filename = file;
        TextAsset textAsset = Resources.Load<TextAsset>("Narration/" + filename);
        if (textAsset != null)
        {
            lines = textAsset.text.Split('\n');
            currentLine = -1;
            narrationText.text = "";
        }
        else
        {
            Debug.LogError("Text file not found in Resources/Narration: " + filename);
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
