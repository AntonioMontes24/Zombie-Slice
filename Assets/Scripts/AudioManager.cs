using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    
    public static AudioManager instance;

    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioSource musicSource;

    public const string MUSIC_KEY = "Music";
    public const string SFX_KEY = "SFX";

    public AudioClip menuMusic;
    public AudioClip gameMusic;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);

            if(musicSource == null)
            {
                musicSource = GetComponent<AudioSource>();
                if(musicSource == null)
                {
                    musicSource = gameObject.AddComponent<AudioSource>();
                }
            }

        } else
        {
            Destroy(gameObject);
        }

        LoadVolume();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentSceneName = scene.name;

        if(currentSceneName == "Main Menu" || currentSceneName == "Options Menu")
        {
            PlayMusic(menuMusic);
        }
        else if (currentSceneName == "Zombie_Scene(Main)")
        {
            PlayMusic(gameMusic);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if(musicSource == null)
        {
            Debug.LogWarning("AudioManager: no audio source assigned for music!");
            return;
        }

        if(clip == null)
        {
            Debug.LogWarning("AudioManager: no music clip!!");
            StopMusic();
            return;
        }

        if(musicSource.clip != clip || !musicSource.isPlaying)
        {
            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
            Debug.Log("Playing Music: { clip.name }");
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    // volume saved in volumesettings.cs
    void LoadVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        mixer.SetFloat(VolumeSettings.MIXER_MUSIC, Mathf.Log10(musicVolume) * 20);
        mixer.SetFloat(VolumeSettings.MIXER_SFX, Mathf.Log10(sfxVolume) * 20);
    }
}
