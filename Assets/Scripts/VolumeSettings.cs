using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSettings : MonoBehaviour
{

    [SerializeField] AudioMixer mixer;
    [SerializeField] public Slider musicSlider;
    [SerializeField] public Slider sfxSlider;
    [SerializeField] public Slider menuSlider;

    public const string MIXER_MUSIC = "Music";
    public const string MIXER_SFX = "SFX";
    public const string MIXER_MENU = "Menu";

    private void OnEnable()
    {
        if(AudioManager.instance == null)
        {
            Debug.LogError("VolumeSettings: Audiomanager instance not found!!");
            return;
        }

        if(musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if(sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        if(menuSlider != null) menuSlider.onValueChanged.AddListener(SetMenuVolume);

        if(musicSlider != null) musicSlider.value = AudioManager.instance.GetMusicVolume();
        if(sfxSlider != null) sfxSlider.value = AudioManager.instance.GetSFXVolume();
        if(menuSlider != null) menuSlider.value = AudioManager.instance.GetMenuVolume();


    }

    private void Start()
    {


    }

    //private void OnEnable()
    //{
    //    musicSlider.onValueChanged.AddListener(SetMusicVolume);
    //    sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    //    menuSlider.onValueChanged.AddListener(SetMenuVolume);

    //}

    private void OnDisable()
    {
        //PlayerPrefs.SetFloat(AudioManager.MUSIC_KEY, musicSlider.value);
        //PlayerPrefs.SetFloat(AudioManager.SFX_KEY, sfxSlider.value);
        //PlayerPrefs.SetFloat(AudioManager.MENU_KEY, menuSlider.value);

        if(musicSlider != null) musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
        if (menuSlider != null) menuSlider.onValueChanged.RemoveListener(SetMenuVolume);

    }

    void SetMusicVolume(float volume)
    {

        float targetVolume = (volume > 0) ? Mathf.Log10(volume) * 20 : -80f;
        mixer.SetFloat(MIXER_MUSIC, targetVolume);
        PlayerPrefs.SetFloat(AudioManager.MUSIC_KEY, volume);
        PlayerPrefs.Save();
    }

    void SetSFXVolume(float volume)
    {
        float targetVolume = (volume > 0) ? Mathf.Log10(volume) * 20 : -80f;
        mixer.SetFloat(MIXER_SFX, targetVolume);
        PlayerPrefs.SetFloat(AudioManager.SFX_KEY, volume);
        PlayerPrefs.Save();
    }

    void SetMenuVolume(float volume)
    {
        float targetVolume = (volume > 0) ? Mathf.Log10(volume) * 20 : -80f;
        mixer.SetFloat(MIXER_MENU, targetVolume);
        PlayerPrefs.SetFloat(AudioManager.MENU_KEY, volume);
        PlayerPrefs.Save();
    }

    public void ResetVolumesToDefault()
    {
        PlayerPrefs.DeleteKey(AudioManager.MENU_KEY);
        PlayerPrefs.DeleteKey(AudioManager.MUSIC_KEY);
        PlayerPrefs.DeleteKey(AudioManager.SFX_KEY);
        PlayerPrefs.Save();

        musicSlider.value = 1f;
        sfxSlider.value = 1f;
        menuSlider.value = 1f;

        if(AudioManager.instance != null)
        {
            AudioManager.instance.ApplySavedVolumeToMixer();
        }



    }

}
