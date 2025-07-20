using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject optionsMenuPanel;

    [SerializeField] private Selectable firstOptionsSelectable;

    [Header("Menu Panels")]
    [SerializeField] GameObject AudioMenu;
    [SerializeField] GameObject VideoMenu;
    [SerializeField] GameObject ControlsMenu;

    [Header("First Selectables for Submenus")]
    [SerializeField] private Selectable firstAudioSelectable;
    [SerializeField] private Selectable firstControlSelectable;
    [SerializeField] private Selectable firstMainMenuSelectable;

    //[Header("Controlers Type")]
    //[SerializeField] GameObject KBMouse;
    //[SerializeField] GameObject controllerPad;

    private GameObject ActiveMenu;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.UI.Enable();
    }

    private void Start()
    {
        AudioMenu.SetActive(false);
        VideoMenu.SetActive(false);
        ControlsMenu.SetActive(false);

        //KBMouse.SetActive(false);
        //controllerPad.SetActive(false);

        if(firstOptionsSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstOptionsSelectable.gameObject);
        }
    }

    private void Update()
    {
        if(inputActions.UI.Cancel.triggered)
        {
            HandleCancel();
        }
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

        //if (ActiveMenu == ControlsMenu)
        //{
        //    ShowKBMouseBindings();
        //}
        //else
        //{
        //    KBMouse.SetActive(false);
        //    controllerPad.SetActive(false);
        //}
    }

    public void AudioOptions()
    {
        SetActiveMenu(AudioMenu);
        ForceSelectFirst(firstAudioSelectable);
    }

    public void VideoOptions()
    {
        SetActiveMenu(VideoMenu);

    }

    public void ControlOptions()
    {
        SetActiveMenu(ControlsMenu);
        ForceSelectFirst(firstControlSelectable);
    }

    //public void ShowKBMouseBindings()
    //{
    //    KBMouse.SetActive(true);
    //    controllerPad.SetActive(false);
    //}

    //public void ShowControllerBindings()
    //{
    //    KBMouse.SetActive(false);
    //    controllerPad.SetActive(true);
    //}

    public void Back()
    {
        if(ActiveMenu != null)
        {
            ActiveMenu.SetActive(false);
            ActiveMenu = null;
        }

        //KBMouse.SetActive(false);
        //controllerPad.SetActive(false);

        if(optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(false);
        }

        if(mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);

        }

        MainMenu mainMenuScript = mainMenuPanel.GetComponent<MainMenu>();
        if (mainMenuScript != null)
        {
            mainMenuScript.FocusMainMenuButton();
        }

        //if (AudioManager.instance != null)
        //{
        //    AudioManager.instance.PlayButtonSelectSound();
        //}
    }

    private void HandleCancel()
    {
        if (ActiveMenu == AudioMenu || ActiveMenu == VideoMenu || ActiveMenu == ControlsMenu)
        {
            // Go back to Options Menu
            SetActiveMenu(null);
            optionsMenuPanel.SetActive(true);
            ForceSelectFirst(firstOptionsSelectable);
        }
        else if (optionsMenuPanel.activeSelf)
        {
            // Go back to Main Menu
            Back();
        }
    }
    public void FocusFirstOption()
    {
        StartCoroutine(DelayedSelect(firstOptionsSelectable));
    }

    private void ForceSelectFirst(Selectable selectable)
    {
        StartCoroutine(DelayedSelect(selectable));
    }

    private IEnumerator DelayedSelect(Selectable selectable)
    {
        yield return null; // wait 1 frame to clear hover state
        EventSystem.current.SetSelectedGameObject(null);

        if (selectable != null)
        {
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }
    }
    public void SelectAfterReturn(Selectable selectable)
    {
        StartCoroutine(DelayedSelect(selectable));
    }
}
