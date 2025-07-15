using JetBrains.Annotations;
using System.Collections;

//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject optionsMenuPanel;

    private void Start()
    {
        if(mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        if(optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(false);
        }

        if(AudioManager.instance != null)
        {
            AudioManager.instance.ApplySavedVolumeToMixer();
        }

    }


    public void PlayGame()
    {

        if(AudioManager.instance != null)
        {
            AudioManager.instance.StopMusic();
        }

        SceneManager.LoadScene(1);


    }

    public void OptionsMenu()
    {
        if(mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        if(optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(true);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }



}
