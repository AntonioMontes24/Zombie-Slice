using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsUI : MonoBehaviour
{
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Main Menu"); 
    }
    public void OpenCreditsScene()
    {
        SceneManager.LoadScene("Credits");
    }
}