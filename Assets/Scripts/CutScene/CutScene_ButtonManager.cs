using UnityEngine;
using UnityEngine.SceneManagement;

public class CutScene_ButtonManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlayButton()
    {
        SceneManager.LoadScene(2);
    }

    public void QuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
}
