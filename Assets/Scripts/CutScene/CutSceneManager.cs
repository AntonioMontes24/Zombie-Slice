using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class CutSceneManager : MonoBehaviour
{
    public PlayableDirector director;
    [SerializeField] public string sceneToLoad; //Serializing for modular configuration
    // [SerializeField] public Slider loadingBar;
    // [SerializeField] private TextMeshProUGUI loadingPercent;
    // [SerializeField] private GameObject loadingUI;
    private bool hasSkipped = false; //Default
    //[SerializeField] public GameObject skipButton;

    private PlayerInputActions inputActions;

    private AsyncOperation loadingOperation;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }


    void Start()
    {
        StartCoroutine(PreloadScene());
    }
    void Update()
    {
        if (!hasSkipped && inputActions.UI.Cancel.triggered)
        {
            SkipCutScene();
        }
    }

    IEnumerator PreloadScene()
    {
        //loadingUI?.SetActive(true); // optional loading UI
        loadingOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        loadingOperation.allowSceneActivation = false;

        while (!loadingOperation.isDone)
        {
            float progress = Mathf.Clamp01(loadingOperation.progress / 0.9f);
            // if (loadingBar != null) loadingBar.value = progress;
            // if (loadingPercent != null) loadingPercent.text = $"Loading... {Mathf.RoundToInt(progress * 100)}";
            if (loadingOperation.progress >= 0.9f)
                break;

            yield return null;
        }
    }

    // IEnumerator FadeOutLoadingUI(float duration = 1f)
    // {
    //     if (loadingUI == null) yield break;

    //     CanvasGroup canvasGroup = loadingUI.GetComponent<CanvasGroup>();
    //     if (canvasGroup == null)
    //     {
    //         Debug.Log("Canvasgroup is missing loadingUI");
    //         canvasGroup = loadingUI.AddComponent<CanvasGroup>();
    //         //yield break;
    //     }

    //     Debug.Log("Starting fade out...");

    //     float startAlpha = canvasGroup.alpha;
    //     float time = 0f;

    //     while (time < duration)
    //     {
    //         time += Time.deltaTime;
    //         canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / duration);
    //         yield return null;
    //     }

    //     canvasGroup.alpha = 0f;
    //     loadingUI.SetActive(false);
    //     Debug.Log("Fade out complete");
    // }


    public void SkipCutScene()
    {
        if (hasSkipped) return;

        hasSkipped = true;

        if (director != null)
        {
            director.Stop();
        }

        SceneManager.LoadScene(sceneToLoad);
    }
    private void OnEnable()
    {
        inputActions.Input.Disable();
        inputActions.UI.Enable();
    }

    private void OnDisable()
    {
        inputActions.Input.Enable();
        inputActions.UI.Disable();
    }


    public void EndCutScene()
    {
        if (hasSkipped) return;
        //skipButton.SetActive(false); //Commented out for now unless porting to mobile
        hasSkipped = true;
        StartCoroutine(FinishCutSceneWithFade());
        //SceneManager.LoadScene(sceneToLoad);
    }

    IEnumerator FinishCutSceneWithFade()
    {
        if (loadingOperation != null)
        {
            loadingOperation.allowSceneActivation = true;

            while (!loadingOperation.isDone)
            {
                yield return null;
            }
        }
        // if (loadingUI != null)
        //     {
        //         yield return StartCoroutine(FadeOutLoadingUI(1.2f));
        //     }

        ActivateLoadedScene();
    }
    
     private void ActivateLoadedScene()
    {
        if (loadingOperation != null)
        {
            loadingOperation.allowSceneActivation = true;
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
