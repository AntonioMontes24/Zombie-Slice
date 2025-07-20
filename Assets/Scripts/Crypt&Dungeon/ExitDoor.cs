using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    public enum ExitType { LoadNextScene, LoadPreviousScene, LoadSpecificScene }

    [SerializeField] private ExitType exitType;
    [SerializeField] private string specificSceneName = ""; // Only used if ExitType is LoadSpecificScene
    [SerializeField] string spawnPointName = "";//Choose a specific spawn point location 

    private bool playerInRange = false;
    private TutorialManager tutorialManager;

    void Start()
    {
        tutorialManager = FindObjectOfType<TutorialManager>();
    }

    void Update()
    {
        if (playerInRange && PlayerController.inputActions.Input.Interact.triggered)
        {
            if (tutorialManager != null && !tutorialManager.IsTutorialComplete)
            {
                Debug.Log("You must complete all tutorial steps before exiting.");
                return;
            }

            // Save spawn point name if specified
            if (!string.IsNullOrEmpty(spawnPointName))
                PlayerPrefs.SetString("LastSpawnPoint", spawnPointName);

            SavePlayerData(); //Save player health and weapon data before changing scene

            switch (exitType)
            {
                case ExitType.LoadNextScene:
                    LoadNext();
                    break;
                case ExitType.LoadPreviousScene:
                    LoadPrevious();
                    break;
                case ExitType.LoadSpecificScene:
                    if (!string.IsNullOrEmpty(specificSceneName))
                        SceneManager.LoadScene(specificSceneName);
                    break;
            }
        }
    }

    private void SavePlayerData()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || PlayerPersistentData.instance == null) return;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        PlayerWeaponManager weaponManager = player.GetComponent<PlayerWeaponManager>();

        float currentHealth = health != null ? health.CurrentHealth : 100f;

        List<WeaponSaveData> weaponData = weaponManager != null
            ? weaponManager.GetWeaponSaveData()
            : new List<WeaponSaveData>();

        PlayerPersistentData.instance.SavePlayerData(currentHealth, weaponData);
    }

    void LoadNext()
    {
        FindObjectOfType<PlayerHealth>().SaveHealth();
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
        else
            Debug.LogWarning("No next scene in build settings.");
    }

    void LoadPrevious()
    {
        int prevIndex = SceneManager.GetActiveScene().buildIndex - 1;
        if (prevIndex >= 0)
            SceneManager.LoadScene(prevIndex);
        else
            Debug.LogWarning("No previous scene in build settings.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
