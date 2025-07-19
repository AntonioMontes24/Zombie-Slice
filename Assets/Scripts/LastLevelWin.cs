using UnityEngine;
using UnityEngine.SceneManagement;

public class LastLevelWin : MonoBehaviour
{


    public string zombieTag = "enemy";

    public GameObject winScreen;

    private bool hasWon = false;



    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        if (SceneManager.GetActiveScene().name != "BossRoom")
        {
            this.enabled = false;
        }
    }


    void Update()
    {
        if (hasWon) return;

        if (GameObject.FindGameObjectsWithTag("enemy").Length == 0 && GameObject.FindGameObjectsWithTag("Boss").Length == 0)
        {
            Win();
        }
    }

    // Update is called once per frame
    void Win()
    {
        hasWon = true;

        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Win Screen UI is not assigned!");
        }

        Time.timeScale = 0f;

        Debug.Log("All enemies cleared! You win!");
    }
}
