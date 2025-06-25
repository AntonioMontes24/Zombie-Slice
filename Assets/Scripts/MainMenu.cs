using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameManager gameManager;

    public void PlayGame(string level)
    {
        SceneManager.LoadScene(level);


    }

    public void OptionsMenu()
    {
        SceneManager.LoadScene(2);
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
