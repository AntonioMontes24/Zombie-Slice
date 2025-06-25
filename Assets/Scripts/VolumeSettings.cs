using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    const string MIXER_MUSIC = "Music";
    const string MIXER_SFX = "SFX";

    private void Awake()
    {
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("Music", Mathf.Log10(value) * 20);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFX", Mathf.Log10(value) * 20);
    }
}
