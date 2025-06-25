using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public Vector3 playerSpawnPoint;

    public Action respawnHook;

    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuOptions;
    [SerializeField] TMP_Text gameTimerText;
    [SerializeField] float remainingTime;
    //[SerializeField] AudioClip musicGame;

    // Player HP Bar info
    public TMP_Text playerHPText;
    public Image playerHPBar;

    // Player Stamina Bar info
    public TMP_Text playerStaminaText;
    public Image playerStaminaBar;


    // Enemy HP Bar info
    public GameObject enemyInfoPanel;
    public TMP_Text enemyHPText;
    public TMP_Text enemyNameText;
    public Image enemyHPBar;

    [SerializeField] public TMP_Text zombieCountText;           
    [SerializeField] public TMP_Text objectiveText;         
                              
    
    
    public GameObject player;
    public PlayerController playerScript;
    public PlayerHealth playerHealth;
    public GameObject flashDamageScreen;
    public GameObject flashHealScreen;
    public GameObject flashAmmoPickUp;

    // AudioSource musicSource;
    // public float musicVolume;

    public bool isPaused;

    float timeScaleOrig;

    //int gameScore;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        playerSpawnPoint = GameObject.FindWithTag("Respawn").transform.position;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        timeScaleOrig = Time.timeScale;

        //musicSource = gameObject.AddComponent<AudioSource>();
        //musicSource.volume = musicVolume;
        //musicSource.clip = musicGame;
        //musicSource.loop = true;
        //musicSource.playOnAwake = false;
        //musicSource.Play();

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
        else if (remainingTime <= 0)
        {
            remainingTime = 0;
            // youWin();
        }

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        gameTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        //if (isPaused && musicSource.isPlaying)
        //{
        //    musicSource.Pause();
        //}
        //else if (!isPaused && !musicSource.isPlaying)
        //{
        //    musicSource.UnPause();
        //}
    }

    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void OptionsMenu()
    {
        menuActive.SetActive(false);
        menuActive = menuOptions;
        menuActive.SetActive(true);
    }

    public void Back()
    {
        menuActive.SetActive(false);
        menuActive = menuPause;
        menuActive.SetActive(true);
    }

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
        flashDamageScreen.SetActive(false);
        // musicSource.Stop();
    }

    public void youWin()
    {
        statePause();
        menuActive = menuWin;
        menuActive.SetActive(true);
        // musicSource.Stop();
    }
}
