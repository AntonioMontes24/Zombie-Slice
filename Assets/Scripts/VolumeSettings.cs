using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSettings : MonoBehaviour
{

    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider menuSlider;

    public const string MIXER_MUSIC = "Music";
    public const string MIXER_SFX = "SFX";
    public const string MIXER_MENU = "Menu";

    public void Awake()
    {
        musicSlider.value = PlayerPrefs.GetFloat(AudioManager.MUSIC_KEY, 1f);
        sfxSlider.value = PlayerPrefs.GetFloat(AudioManager.SFX_KEY, 1f);
        menuSlider.value = PlayerPrefs.GetFloat(AudioManager.MENU_KEY, 1f);



    }

    private void Start()
    {


    }

    private void OnEnable()
    {
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        menuSlider.onValueChanged.AddListener(SetMenuVolume);

    }

    private void OnDisable()
    {
        //PlayerPrefs.SetFloat(AudioManager.MUSIC_KEY, musicSlider.value);
        //PlayerPrefs.SetFloat(AudioManager.SFX_KEY, sfxSlider.value);
        //PlayerPrefs.SetFloat(AudioManager.MENU_KEY, menuSlider.value);

        musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
        sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
        menuSlider.onValueChanged.RemoveListener(SetMenuVolume);
    }

    void SetMusicVolume(float volume)
    {
        mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(AudioManager.MUSIC_KEY, volume);
    }

    void SetSFXVolume(float volume)
    {
        mixer.SetFloat(MIXER_SFX, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(AudioManager.SFX_KEY, volume);
    }

    void SetMenuVolume(float volume)
    {
        mixer.SetFloat(MIXER_MENU, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(AudioManager.MENU_KEY, volume);
    }

}
