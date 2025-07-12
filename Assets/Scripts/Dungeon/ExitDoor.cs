using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    public enum ExitType { LoadNextScene, LoadPreviousScene, LoadSpecificScene }

    [SerializeField] private ExitType exitType;
    [SerializeField] private string specificSceneName = ""; // Only used if ExitType is LoadSpecificScene

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
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

    void LoadNext()
    {
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
