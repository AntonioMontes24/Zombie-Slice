using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ButtonFunctions : MonoBehaviour
{
   public void resume()
    {
        GameManager.instance.stateUnpause();
    }
    public void restart()
    {
        //GameManager.instance.respawnHook?.Invoke();
        //GameManager.instance.player.GetComponent<PlayerController>().SpawnPlayer();
        //GameManager.instance.stateUnpause();

        //SceneManager.sceneLoaded += OnSceneReloaded;

        //SceneManager.LoadScene("Zombie_Scene(Main)");

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);

        //if (KillManager.instance != null)
        //{
        //    KillManager.instance.ResetKills();
        //}
    }

    public void Respawn()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            
            GameManager.instance.player.transform.position = GameManager.instance.playerSpawnPoint;

           
            GameManager.instance.player.GetComponent<PlayerHealth>().Revive();

            GameManager.instance.statePause();

            GameManager.instance.stateUnpause();
        }
    }



    private void OnSceneReloaded(Scene scene, LoadSceneMode mode)  //makes sure you reload the scene with the barriers getting reset
    {

        SceneManager.sceneLoaded -= OnSceneReloaded;

        if (KillManager.instance != null)
        {
            KillManager.instance.ResetKills();
        }

        if (GameManager.instance != null) {

            GameManager.instance.ResetBarriers();
            GameManager.instance.stateUnpause();

        
        }
   }


    public void options()
    {
        // saving this for later
        if (GameManager.instance != null)
        {
            GameManager.instance.OpenOptionsMenu();
        }
    }

    public void audioOptions()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OpenAudioOptionsMenu();
        }
        {
            
        }
    }

    public void videoOptions()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OpenVideoOptionsMenu();
        }
        {

        }
    }
    public void controlsOptions()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OpenControlsOptionsMenu();
        }
        {

        }
    }

    public void quit()
    {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else

        Application.Quit();
#endif
    }

    [Header("Door Interaction")]
    [SerializeField] private float holdDuration = 1f;
    [SerializeField] private int sceneBuildIndex = 2;
    //[SerializeField] private KeyCode interactKey = KeyCode.X;
    [SerializeField] private TMP_Text InteractText;

    [Header("UI")]
    [SerializeField] private TMP_Text interactPrompt;

  
    private bool isPlayerNearDoor = false;

    private void Start()
    {
    

    }


    private void Update()
    {
        

    }

   
}
