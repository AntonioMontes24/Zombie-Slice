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

    [Header("Controlers Type")]
    [SerializeField] GameObject KBMouse;
    [SerializeField] GameObject controllerPad;

    private GameObject ActiveMenu;

    private void Start()
    {
        AudioMenu.SetActive(false);
        VideoMenu.SetActive(false); 
        ControlsMenu.SetActive(false);

        KBMouse.SetActive(false);
        controllerPad.SetActive(false);
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

        if(ActiveMenu == ControlsMenu)
        {
            ShowKBMouseBindings();
        }
        else
        {
            KBMouse.SetActive(false);
            controllerPad.SetActive(false);
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

    public void ShowKBMouseBindings()
    {
        KBMouse.SetActive(true);
        controllerPad.SetActive(false);
    }

    public void ShowControllerBindings()
    {
        KBMouse.SetActive(false);
        controllerPad.SetActive(true);
    }


    public void Back()
    {
        if(ActiveMenu != null)
        {
            ActiveMenu.SetActive(false);
            ActiveMenu = null;
        }

        KBMouse.SetActive(false);
        controllerPad.SetActive(false);


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
