using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CinematicController : MonoBehaviour
{
    public PlayableDirector director;
    public string nextSceneName;
    public bool useTimelineSignal = false;
    private bool isFadingOut;
    private float fadeTimer;
    private float initialVolume = 1f;
    private bool hasPlayed = false;
    private float fadeDuration = 2f;

    [Header("CutScene Music")]
    public AudioSource musicSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (director != null && !useTimelineSignal)
        {
            director.Play();
            director.stopped += OnTimeLineFinished;
        }

        if (musicSource != null && musicSource.clip != null)
        {
            initialVolume = musicSource.volume;
            musicSource.Play();
        }
    }

    public void TriggerMusicFade()
    {
        if (!isFadingOut && musicSource != null)
        {
            isFadingOut = true;
            fadeTimer = 1f;
            initialVolume = musicSource.volume;
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
        if (isFadingOut && musicSource != null)
        {
            fadeTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(fadeTimer / fadeDuration);
            musicSource.volume = Mathf.Lerp(initialVolume, 0f, progress);
        }
    }
}
