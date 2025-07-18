using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NarrationController : MonoBehaviour
{
    public TextAsset narrationFile;
    public GameObject textLinePrefab; //TMP components
    public Transform contenParent;
    [SerializeField] public float scrollSpeed = 30f; // Default to 30f, adjustable in Inspector
    [SerializeField] public float fadeDistance = 100f; //Default to 100f can adjust in inspector

    private List<TextMeshProUGUI> lines = new List<TextMeshProUGUI>();

    void Start()
    {
        if (narrationFile == null)
        {
            narrationFile = Resources.Load<TextAsset>("Narration/IntroNarration");
        }

        string[] textLines = narrationFile.text.Split('\n');

        foreach (string line in textLines)
        {
            GameObject obj = Instantiate(textLinePrefab, contenParent);
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = line.Trim();
            lines.Add(tmp);
        }

        StartCoroutine(ScrollandFade());
    }

    IEnumerator ScrollandFade()
    {
        while (true)
        {
            contenParent.transform.localPosition += Vector3.up * scrollSpeed * Time.deltaTime;

            foreach (var line in lines)
            {
                if (line == null) continue;

                float screenY = line.transform.position.y;

                float canvasHeight = Screen.height;

                float distanceFromBottom = screenY;
                float distanceFromTop = canvasHeight - screenY;

                float fade = 1f;

                if (distanceFromBottom < fadeDistance)
                    fade = Mathf.Clamp01(distanceFromBottom / fadeDistance);
                else if (distanceFromTop < fadeDistance)
                    fade = Mathf.Clamp01(distanceFromTop / fadeDistance);

                Color color = line.color;
                color.a = fade;
                line.color = color;
            }
            yield return null;
        }
    }
}
