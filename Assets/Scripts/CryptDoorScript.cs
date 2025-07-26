using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Collections;

public class CryptDoorScript : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string spawnPointName = "";
    [SerializeField] private int nextSceneIndex;

    [SerializeField] private int objectiveIndexToCheck = 0;

    [SerializeField] private string objectiveNotCompleteMessage;
    [SerializeField] private float messageDisplayDuration = 3f;

    [SerializeField] private GameObject interactionPromptUI;
    [SerializeField] private TextMeshProUGUI messageTexTUI;

    private bool playerInRange = false;
    private Coroutine messageCoroutin;

    private void Awake()
    {
        if(interactionPromptUI != null) interactionPromptUI.SetActive(false);
        if(messageTexTUI != null) messageTexTUI.gameObject.SetActive(false);

        if(nextSceneIndex == 0)
        {
            nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(playerInRange && PlayerController.inputActions.Input.Interact.triggered)
        {
            if(ObjectiveManager.instance == null)
            {
                return;
            }

            if (!ObjectiveManager.instance.IsObjectiveComplete(objectiveIndexToCheck))
            {
                DisplayMessage(objectiveNotCompleteMessage);
                return;
            }
            LoadCryptScene();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if(interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if(interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(false);
            }
            if(messageCoroutin != null)
            {
                StopCoroutine(messageCoroutin);
                messageTexTUI.gameObject.SetActive(false);
            }
        }
    }

    void LoadCryptScene()
    {
        if (!string.IsNullOrEmpty(spawnPointName))
        {
            PlayerPrefs.SetString("LastSpawnPoint", spawnPointName);
        }

        if(nextSceneIndex >= 0 && nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    void DisplayMessage(string message)
    {
        if(messageTexTUI != null)
        {
            if(messageCoroutin != null)
            {
                StopCoroutine(messageCoroutin);
            }
            messageTexTUI.text = message;
            messageTexTUI.gameObject.SetActive(true);
            messageCoroutin = StartCoroutine(HideMessageAfterDelay(messageDisplayDuration));
        }
    }

    IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if(messageTexTUI != null)
        {
            messageTexTUI.gameObject.SetActive(false);
        }
    }


}
