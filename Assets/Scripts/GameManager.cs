using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Vector3 playerSpawnPoint;

    public Action respawnHook;

    public TextMeshProUGUI ammoText;

    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuOptions;
    [SerializeField] GameObject menuNoTime;
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
    public TMP_Text enemyNameText;
    public Image enemyHPBar;

    [SerializeField] public TMP_Text objectiveText;   
                              
    public GameObject player;
    public PlayerController playerScript;
    public PlayerHealth playerHealth;
    public GameObject flashDamageScreen;
    public GameObject flashHealScreen;
    public GameObject flashAmmoPickUp;

    //AudioSource musicSource;
    //public float musicVolume;

    public bool isPaused;

    float timeScaleOrig;

    //int gameScore;

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
        var respawnPoint = GameObject.FindWithTag("Respawn");
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        if (respawnPoint == null)
            playerSpawnPoint = player.transform.position;
        else
            playerSpawnPoint = respawnPoint.transform.position;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if(enemyInfoPanel != null )
        {
            enemyInfoPanel.SetActive(false);
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
            } else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }
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
        if(menuActive != null)
        {
            menuActive.SetActive(false);
            menuActive = null;
        }
    }

    public void OptionsMenu()
    {
        if(menuActive != null)
        {
            menuActive.SetActive(false);
        }
        menuActive = menuOptions;
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

}
