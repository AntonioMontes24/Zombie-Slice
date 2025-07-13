using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Vector3 playerSpawnPoint;

    public Action respawnHook;

    //public TextMeshProUGUI ammoText;

    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuOptions;
    [SerializeField] GameObject menuNoTime;

    [SerializeField] GameObject menuAudio;
    [SerializeField] GameObject menuVideo;
    [SerializeField] GameObject menuControls;

    [SerializeField] GameObject inGameUI;


    [SerializeField] TMP_Text gameTimerText;
    [SerializeField] float remainingTime;

    // Player HP Bar info
    public TMP_Text playerHPText;
    public Image playerHPBar;

    // Player Stamina Bar info
    public TMP_Text playerStaminaText;
    public Image playerStaminaBar;

    // Ammo Bar Info
    public TMP_Text ammoText;
    public Image ammoyBar;

    // Enemy HP Bar info
    public GameObject enemyInfoPanel;
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

    public bool isPaused;

    float timeScaleOrig;

    private iEnemyHealth currentEnemy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {

        if(instance == null)
        {
            instance = this;
        } else if(instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Time.timeScale = 1.0f;
        timeScaleOrig = Time.timeScale;

        //player and spawn point
        var respawnPoint = GameObject.FindWithTag("Respawn");
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        if (respawnPoint == null)
            playerSpawnPoint = player.transform.position;
        else
            playerSpawnPoint = respawnPoint.transform.position;

        // cursor state for gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // make sure enemy info panel is hidden initially
        if(enemyInfoPanel != null )
        {
            enemyInfoPanel.SetActive(false);
        }

        //make sure all menus are initially inactive
        if(menuPause != null)
        {
            menuPause.SetActive(false);
        }
        if (menuWin != null)
        {
            menuPause.SetActive(false);
        }
        if (menuLose != null)
        {
            menuPause.SetActive(false);
        }
        if (menuOptions != null)
        {
            menuPause.SetActive(false);
        }
        if (menuNoTime != null)
        {
            menuPause.SetActive(false);
        }

        // make sure ingame ui is active at start
        if(inGameUI != null)
        {
            inGameUI.SetActive(true);
        }

    }

    private void Start()
    {
        if(AudioManager.instance != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if(currentSceneName == "Main Menu" || currentSceneName == "Options Menu")
            {
                AudioManager.instance.PlayMusic(AudioManager.instance.menuMusic);
            } else if (currentSceneName == "Zombie_Scene(Main)")
            {
                AudioManager.instance.PlayMusic(AudioManager.instance.gameMusic);
            }
        }
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

                // hide all ingame ui
                if(inGameUI != null)
                {
                    inGameUI.SetActive(false);
                }


            } else if (menuActive == menuPause)
            {
                stateUnpause();
            }

            else if(menuActive == menuOptions)
            {
                BackToPauseMenu();
            }
        }

        // only update game timer if not paused
        if (!isPaused)
        {
            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
            }
            else if (remainingTime <= 0)
            {
                remainingTime = 0;
                if (!isPaused)
                {
                    youRanOutOfTime();
                }
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.StopMusic();
                }
            }

        }

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        gameTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

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

        //hide current menu
        if(menuActive != null)
        {
            menuActive.SetActive(false);
            menuActive = null;
        }

        // show ingame ui again
        if(inGameUI != null)
        {
            inGameUI.SetActive(true);
        }
    }

    public  void BackToPauseMenu()
    {
        if(menuActive != null)
        {
            menuActive.SetActive(false);
        }
        menuActive = menuOptions;
        menuActive.SetActive(true);
    }

    public void OpenOptionsMenu()
    {
        if(menuActive != null)
        {
            menuActive.SetActive(false);
        }
        menuActive = menuOptions;
        menuActive.SetActive(true);
    }

    public void OpenAudioOptionsMenu()
    {
        if (menuActive != null)
        {
            menuActive.SetActive(false);
        }
        menuActive = menuAudio;
        menuActive.SetActive(true);

    }

    public void OpenVideoOptionsMenu()
    {
        if (menuActive != null)
        {
            menuActive.SetActive(false);
        }
        menuActive = menuVideo;
        menuActive.SetActive(true);
    }

    public void OpenControlsOptionsMenu()
    {
        if (menuActive != null)
        {
            menuActive.SetActive(false);
        }
        menuActive = menuControls;
        menuActive.SetActive(true);
    }

    public void Back()
    {
        if(menuActive != null)
        {
            menuActive.SetActive(false);
        }
        menuActive = menuPause;
        menuActive.SetActive(true);
    }

    public void youLose()
    {
        statePause();

        //hide ingame ui when you have lost
        if(inGameUI != null)
        {
            inGameUI.SetActive(false);
        }
        AudioManager.instance.StopMusic();
        menuActive = menuLose;
        menuActive.SetActive(true);
        if(flashDamageScreen != null)
        {
            flashDamageScreen.SetActive(false);
        }

    }

    public void youRanOutOfTime()
    {
        statePause();

        if(inGameUI != null)
        {
            inGameUI.SetActive(false);
        }
        AudioManager.instance.StopMusic();
        menuActive = menuNoTime;
        menuActive.SetActive(true);
        if(flashDamageScreen != null)
        {
            flashDamageScreen.SetActive(false);
        }
    }

    public void youWin()
    {
        statePause();
        if(inGameUI != null)
        {
            inGameUI.SetActive(false);
        }
        menuActive = menuWin;
        menuActive.SetActive(true);

    }

    public void SetCurrentEnemy(iEnemyHealth en)
    {
        currentEnemy = en;
        if (currentEnemy != null)
        {
            enemyInfoPanel.SetActive(true);
        }

        if(enemyNameText != null && en != null && (en as MonoBehaviour) != null)
        {
            enemyNameText.text = (en as MonoBehaviour).gameObject.name;
        }
        UpdateEnemyHealthBar(en);

    }

    public void UpdateEnemyHealthBar(iEnemyHealth en)
    {
        if(currentEnemy == en && en.maxHealth > 0)
        {
            if(enemyHPBar != null)
            {
                enemyHPBar.fillAmount = (float)en.CurrentHealth / en.maxHealth;
            }  
        }
        else if (currentEnemy == en && en.CurrentHealth <= 0)
        {
            HideEnemyUI();
        }
    }

    public void HideEnemyUI()
    {
        if(enemyInfoPanel != null)
        {
            enemyInfoPanel.SetActive(false);
        }
        currentEnemy = null;
    }

    public void GoToMainMenu()
    {
        stateUnpause();
        SceneManager.LoadScene("Main Menu");
    }

}
