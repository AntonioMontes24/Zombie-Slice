using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NarrationController : MonoBehaviour
{
    public TextAsset narrationFile;
    public GameObject textLinePrefab; //TMP components
    public GameObject creditsLinePrefab;
    public Transform contenParent;
    public Transform creditsParent;
    [SerializeField] public float scrollSpeed = 30f; // Default to 30f, adjustable in Inspector
    [SerializeField] public float fadeDistance = 100f; //Default to 100f can adjust in inspector
    [SerializeField] private bool loadCreditsAfterScroll = false; //Default to false
    [SerializeField] private TextAsset creditsFile;
    [SerializeField] public string sceneToLoad;
    public enum NarrationMode { Scroll, FadeCredits }
    public NarrationMode mode = NarrationMode.Scroll;

    private List<TextMeshProUGUI> lines = new List<TextMeshProUGUI>();
    private bool hasSkipped;
    private PlayerInputActions InputActions;

    private void Awake()
    {
        InputActions = new PlayerInputActions();
    }

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
    void Update()
    {
        if (!hasSkipped && InputActions.UI.Cancel.triggered)
        {
            SkipScene();
        }
    }

    public void SkipScene()
    {
        if (hasSkipped) return;
        hasSkipped = true;
        SceneManager.LoadScene(sceneToLoad);
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


            if (mode == NarrationMode.Scroll && loadCreditsAfterScroll)
            {
                float lastLineY = lines[^1].transform.position.y;
                if (lastLineY > Screen.height + 100f)
                {
                    loadCreditsAfterScroll = false;
                    StartCoroutine(LoadCreditsAfterScroll());
                }
            }
            yield return null;
        }
    }

    IEnumerator LoadCreditsAfterScroll()
    {
        yield return new WaitForSeconds(0.5f);//Scroll delay afterwards

        //clear lines
        foreach (var l in lines)
            Destroy(l.gameObject);
        lines.Clear();

        //Load credits
        string[] creditLines = creditsFile.text.Split('\n');
        foreach (string line in creditLines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var parent = creditsParent != null ? creditsParent : contenParent;
                var prefab = creditsLinePrefab != null ? creditsLinePrefab : textLinePrefab;
                var obj = Instantiate(prefab, parent);
                var tmp = obj.GetComponent<TextMeshProUGUI>();
                tmp.text = line.Trim();
                tmp.color = new Color(1, 1, 1, 0);
                obj.SetActive(false);
                var rt = obj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(Random.Range(-200f, 200f), Random.Range(-100f, 100f));

                lines.Add(tmp);
            }
        }

        mode = NarrationMode.FadeCredits;
        if (contenParent is RectTransform rectTransform)
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }
        StartCoroutine(FadeCreditsSequence());
    }

    IEnumerator FadeCreditsSequence()
    {
        int groupSize = 4; //number of lines
        float fadeDuration = 0.5f;
        float visibleDuration = 3f;

        for (int i = 0; i < lines.Count; i += groupSize)
        {
            int max = Mathf.Min(i + groupSize, lines.Count);
            List<TextMeshProUGUI> group = new List<TextMeshProUGUI>();

            for (int j = i; j < max; j++)
            {
                var tmp = lines[j];
                tmp.gameObject.SetActive(true);
                StartCoroutine(FadeText(tmp, 0f, 1f, fadeDuration));
                group.Add(tmp);
            }

            yield return new WaitForSeconds(visibleDuration + fadeDuration);

            foreach (var tmp in group)
            {
                StartCoroutine(FadeText(tmp, 1f, 0f, fadeDuration));
            }

            yield return new WaitForSeconds(fadeDuration);

            foreach (var tmp in group)
            {
                tmp.gameObject.SetActive(false);
            }
        }

        yield return new WaitForSeconds(1f);

        ShowEndOptions();
    }

    IEnumerator FadeText(TextMeshProUGUI text, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            Color c = text.color;
            c.a = alpha;
            text.color = c;
            yield return null;
        }

        Color final = text.color;
        final.a = to;
        text.color = final;
    }

    [SerializeField] private GameObject endOptionsUI;

    void ShowEndOptions()
    {
        if (endOptionsUI != null)
        {
            endOptionsUI.SetActive(true);
        }
    }
}
