using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public string level = "Zombie_Scene(Main)";


    public void PlayeGame()
    {
        if (mainMenu != null)
        {
            mainMenu.SetActive(true);
        }

        SceneManager.LoadScene(level);


    }

    public void OptionsMenu()
    {

    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
