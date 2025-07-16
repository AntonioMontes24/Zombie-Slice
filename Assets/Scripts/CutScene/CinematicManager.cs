using UnityEngine;

public class CinematicManager : MonoBehaviour
{
    public GameObject playerUI;
    public GameObject cutsceneEnemies;
    public GameObject regularEnemies;
    public GameObject playerCamera;
    public GameObject virtualCamera;

    void Start()
    {
        playerUI.SetActive(false);
        cutsceneEnemies.SetActive(true);
        regularEnemies.SetActive(false);
        playerCamera.SetActive(false);
        virtualCamera.SetActive(true);
    }

    public void EndCinematic()
    {
        playerUI.SetActive(true);
        cutsceneEnemies.SetActive(false);
        regularEnemies.SetActive(true);
        virtualCamera.SetActive(false);
        playerCamera.SetActive(true);
    }
}
