using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CinematicController : MonoBehaviour
{
    public PlayableDirector director;
    public string nextSceneName;
    public bool useTimelineSignal = false;

    private bool hasPlayed = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (director != null && !useTimelineSignal)
        {
            director.Play();
            director.stopped += OnTimeLineFinished;
        }
    }

    private void OnTimeLineFinished(PlayableDirector obj)
    {
        if (!hasPlayed)
        {
            hasPlayed = true;
            LoadNextScene();
        }
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
