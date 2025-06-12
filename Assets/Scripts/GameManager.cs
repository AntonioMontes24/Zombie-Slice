using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] TMP_Text gameTimerText;
    [SerializeField] float remainingTime;
    //[SerializeField] TMP_Text gameScoreText;
    [SerializeField] AudioClip musicGame;


    //public GameObject playerDamageScreen;
    //public Image playerHealthBar;

    public Image playerHPBar;
    public GameObject player;
    public PlayerController playerScript;
    public GameObject flashDamageScreen;
    public GameObject flashHealScreen;
    public GameObject flashAmmoPickUp;

    public bool isPaused;

    float timeScaleOrig;

    //int gameScore;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        timeScaleOrig = Time.timeScale;

        

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if(menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(isPaused);
            } else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }

        if(remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
        } 
        else if (remainingTime < 0)
        {
            remainingTime = 0;
            youWin();
        }

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        gameTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

    }

    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void stateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
        flashDamageScreen.SetActive(false);
    }

    public void youWin()
    {
        statePause();
        menuActive = menuWin;
        menuActive.SetActive(true);
    }



}
