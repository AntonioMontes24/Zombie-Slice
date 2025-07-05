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

        SceneManager.sceneLoaded += OnSceneReloaded;

        SceneManager.LoadScene("Zombie_Scene(Main)");

        //if (KillManager.instance != null)
        //{
        //    KillManager.instance.ResetKills();
        //}
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
            GameManager.instance.OptionsMenu();
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
    [SerializeField] private KeyCode interactKey = KeyCode.X;
    [SerializeField] private TMP_Text InteractText;

    //private float holdTimer = 0f;
    private bool isPlayerNearDoor = false;

    private void Update()
    {
        //if (isPlayerNearDoor && ObjectiveManager.instance != null && ObjectiveManager.instance.GetZombieCount() <= 1)
        //{
        //    InteractText.gameObject.SetActive(true);

        //    if (Input.GetKey(interactKey))
        //    {
        //        holdTimer += Time.deltaTime;

        //        if (holdTimer >= holdDuration)
        //        {
        //            LoadTargetScene();
        //        }
        //    }
        //    else
        //    {
        //        holdTimer = 0f;
        //    }
        //}

        //else
        //{
        //    InteractText.gameObject.SetActive(false);
        //    holdTimer = 0f;
        //}
        
    }

    private void LoadTargetScene()
    {
        Debug.Log("Loading scene: " + sceneBuildIndex);
        SceneManager.LoadScene(sceneBuildIndex);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearDoor = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearDoor = false;
            //holdTimer = 0f;
        }
    }
}
