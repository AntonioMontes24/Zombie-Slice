using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    
    public static AudioManager instance;

    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioSource musicSource;

    [SerializeField] AudioSource buttonHoverSource;
    [SerializeField] AudioSource buttonSelectSource;

    public const string MUSIC_KEY = "Music";
    public const string SFX_KEY = "SFX";
    public const string MENU_KEY = "Menu";

    public AudioClip menuMusic;
    public AudioClip gameMusic;

    public AudioClip buttonHoverClip;
    public AudioClip buttonSelectClip;

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

            // button audio sources
            if(buttonHoverSource == null)
            {
                //GameObject hover = GameObject.Find("ButtonHover");
                Transform hoverTransform = transform.Find("ButtonHover");
                if(hoverTransform != null)
                {
                    buttonHoverSource = hoverTransform.GetComponent<AudioSource>();
                    if(buttonHoverSource == null)
                    {
                        //buttonHoverSource= gameObject.AddComponent<AudioSource>();
                        buttonHoverSource = hoverTransform.gameObject.AddComponent<AudioSource>();
                    }
                }else
                {
                    Debug.LogWarning("AudioManager: button hover gameobject not found");
                }
            }

            if(buttonSelectClip == null)
            {
                Transform selectTransform = transform.Find("ButtonSelect");
                //GameObject select = GameObject.Find("ButtonSelect");
                if(selectTransform != null)
                {
                    buttonSelectSource = selectTransform.GetComponent<AudioSource>();
                    if(buttonSelectSource == null)
                    {
                        //buttonSelectSource= gameObject.AddComponent<AudioSource>();
                        buttonSelectSource = selectTransform.gameObject.AddComponent<AudioSource>();
                    }
                }else
                {
                    Debug.LogWarning("AudioManager: button select gameobject not found!");
                }
            }

            if(buttonHoverSource != null && mixer != null)
            {
                string menuGroupName = "Menu";
                AudioMixerGroup[] menuGroups = mixer.FindMatchingGroups(menuGroupName);
                if(menuGroups.Length > 0)
                {
                    buttonHoverSource.outputAudioMixerGroup = menuGroups[0];
                }
                //buttonHoverSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Menu")[0];
            }
            if(buttonSelectSource != null && mixer != null)
            {
                string menuGroupName = "Menu";
                AudioMixerGroup[] menuGroups = mixer.FindMatchingGroups(menuGroupName);

                if(menuGroups.Length > 0)
                {
                    buttonSelectSource.outputAudioMixerGroup = menuGroups[0];
                }
                //buttonSelectSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Menu")[0];
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

    public void PlayButtonHoverSound()
    {
        if(buttonHoverSource != null && buttonHoverClip != null)
        {
            buttonHoverSource.PlayOneShot(buttonHoverClip);
        } else
        {
            Debug.LogWarning("AudioManager: button hover sound not set up correctly.");
        }
    }

    public void PlayButtonSelectSound()
    {
        if(buttonSelectSource != null && buttonSelectClip != null)
        {
            buttonSelectSource.PlayOneShot(buttonSelectClip);
        } else
        {
            Debug.LogWarning("AudioManager: button select sound not set up correctly.");
        }
    }


    // volume saved in volumesettings.cs
    void LoadVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);
        float menuVolume = PlayerPrefs.GetFloat(MENU_KEY, 1f);

        mixer.SetFloat(VolumeSettings.MIXER_MUSIC, Mathf.Log10(musicVolume) * 20);
        mixer.SetFloat(VolumeSettings.MIXER_SFX, Mathf.Log10(sfxVolume) * 20);
        mixer.SetFloat(VolumeSettings.MIXER_MENU, Mathf.Log10(menuVolume) * 20);
    }
}
