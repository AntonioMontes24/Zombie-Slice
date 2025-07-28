using JetBrains.Annotations;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject optionsMenuPanel;
    [Header("Controller Navigation")]
    [SerializeField] private GameObject firstSelectedButton; // for controller nav
    public OptionsMenu optionsMenuScript; // assign via Inspector
     public GameObject WebGLMessage;
    public GameObject mainMenuButton;
    private void Start()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        if (optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(false);
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.ApplySavedVolumeToMixer();
        }
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }


    public void PlayGame()
    {

        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopMusic();
        }

        SceneManager.LoadScene(2); // Note from William --> Changed loadscene from 1 to 2, due to branding scene placed at 0 Main Menu is now 1


    }

    public void OptionsMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        if (optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(true);

            OptionsMenu optionsScript = optionsMenuPanel.GetComponent<OptionsMenu>();
            if (optionsScript != null)
            {
                optionsScript.FocusFirstOption();
            }
        }
    }



    public void ReturnFromOptions()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (optionsMenuScript != null && firstSelectedButton != null)
            optionsMenuScript.SelectAfterReturn(firstSelectedButton.GetComponent<Selectable>());
    }
    public void FocusMainMenuButton()
    {
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    public void QuitGame()
    {
#if UNITY_WEBGL
        {
            mainMenuButton.SetActive(false);
            WebGLMessage.SetActive(true);

        }
#elif UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif

    }
}