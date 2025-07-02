using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
//using Unity.VisualScripting;

public class VolumeSettings : MonoBehaviour
{

    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    public const string MIXER_MUSIC = "Music";
    public const string MIXER_SFX = "SFX";

    private void Start()
    {
        musicSlider.value = PlayerPrefs.GetFloat(AudioManager.MUSIC_KEY, 1f);
        sfxSlider.value = PlayerPrefs.GetFloat(AudioManager.SFX_KEY, 1f);
    }

    public void Awake()
    {
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(AudioManager.MUSIC_KEY, musicSlider.value);
        PlayerPrefs.SetFloat(AudioManager.SFX_KEY, sfxSlider.value);
    }

    void SetMusicVolume(float volume)
    {
        mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(volume) * 20);
    }

    void SetSFXVolume(float volume)
    {
        mixer.SetFloat(MIXER_SFX, Mathf.Log10(volume) * 20);
    }

}
