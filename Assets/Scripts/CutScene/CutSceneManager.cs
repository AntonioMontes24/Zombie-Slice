using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutSceneManager : MonoBehaviour
{
    public PlayableDirector director;
    [SerializeField] public string sceneToLoad; //Serializing for modular configuration
    private bool hasSkipped = false; //Default
    //public GameObject skipButton;

    void Update()
    {
        if (!hasSkipped && Input.GetKeyDown(KeyCode.Escape))
        {
            SkipCutScene();
        }
    }

    public void SkipCutScene()
    {
        if (hasSkipped) return;

        hasSkipped = true;

        if (director != null)
        {
            director.Stop();
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    public void EndCutScene()
    {
        if (hasSkipped) return;
        //skipButton.SetActive(false); //Commented out for now unless porting to mobile
        hasSkipped = true;
        SceneManager.LoadScene(sceneToLoad);
    }
}
