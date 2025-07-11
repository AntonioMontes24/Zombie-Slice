using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{

    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject optionsMenuPanel;

    [Header("Menu Panels")]
    [SerializeField] GameObject AudioMenu;
    [SerializeField] GameObject VideoMenu;
    [SerializeField] GameObject ControlsMenu;

    private GameObject ActiveMenu;

    private void Start()
    {
        AudioMenu.SetActive(false);
        VideoMenu.SetActive(false); 
        ControlsMenu.SetActive(false);
    }

    private void SetActiveMenu(GameObject menu)
    {
        if(ActiveMenu != null)
        {
            ActiveMenu.SetActive(false);
        }

        ActiveMenu = menu;

        if(ActiveMenu != null)
        {
            ActiveMenu.SetActive(true);
        }
    }

    public void AudioOptions()
    {
        SetActiveMenu(AudioMenu);
        
    }

    public void VideoOptions()
    {
        SetActiveMenu(VideoMenu);

    }

    public void ControlOptions()
    {
        SetActiveMenu(ControlsMenu);

    }


    public void Back()
    {
        if(ActiveMenu != null)
        {
            ActiveMenu.SetActive(false);
            ActiveMenu = null;
        }
        if(optionsMenuPanel != null)
            {
                optionsMenuPanel.SetActive(false);
            }
            if(mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
        
        //if(AudioManager.instance != null)
        //{
        //    AudioManager.instance.PlayButtonSelectSound();
        //}

    }
}
