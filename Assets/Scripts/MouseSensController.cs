using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MouseSensController : MonoBehaviour
{

    [Header("UI Elements")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_InputField sensitivityInputField;
    [SerializeField] private Button resetButton;

    [Header("Settings")]
    [SerializeField] private float defaultSens = 40.0f;
    [SerializeField] private float minSens = 10.0f;
    [SerializeField] private float maxSens = 70.0f;

    private const string SensPlayerPrefKey = "MouseSensitivity";

    public static event Action<float> OnSensChanged;

    private void Awake()
    {
        if (sensitivitySlider == null) Debug.LogError("Sens Slider not assigned!!");
        if (sensitivityInputField == null) Debug.LogError("Input box not assigned!!");
        if (resetButton == null) Debug.LogError("Reset button not assigned!!");
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // load sens
        float loadedSens = PlayerPrefs.GetFloat(SensPlayerPrefKey, defaultSens);

        loadedSens = Mathf.Clamp(loadedSens, minSens, maxSens);

        //initialize ui with loaded values
        sensitivitySlider.minValue = minSens;
        sensitivitySlider.maxValue = maxSens;
        UpdateUI(loadedSens);

        //add listeners for ui
        sensitivitySlider.onValueChanged.AddListener(OnSliderValueChanged);
        sensitivityInputField.onEndEdit.AddListener(OnInputFieldValueChanged);
        resetButton.onClick.AddListener(ResetSensitivity);

    }

    private void OnDestroy()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
        if(sensitivityInputField != null)
        {
            sensitivityInputField.onEndEdit.RemoveListener(OnInputFieldValueChanged);
        }
        if(resetButton != null)
        {
            resetButton.onClick.RemoveListener(ResetSensitivity);
        }
    }

    private void UpdateUI(float newSens)
    {
        sensitivitySlider.value = newSens;

        sensitivityInputField.text = newSens.ToString("F1");

        PlayerPrefs.SetFloat(SensPlayerPrefKey, newSens);
        PlayerPrefs.Save();

        OnSensChanged?.Invoke(newSens);

        Debug.Log($"Mouse Sens changed to: {newSens}.");

    }

    private void OnSliderValueChanged(float value)
    {
        UpdateUI(value);
    }

    private void OnInputFieldValueChanged(string text)
    {
        if(float.TryParse(text, out float parsedSensitivity))
        {
            parsedSensitivity = Mathf.Clamp(parsedSensitivity, minSens, maxSens);
            UpdateUI(parsedSensitivity);
        }else
        {
            sensitivityInputField.text = sensitivitySlider.value.ToString("F1");
            Debug.LogWarning("Invalid input for mouse sens. Please enter a number.");
        }
    }

    public void ResetSensitivity()
    {
        UpdateUI(defaultSens);
    }

    public float GetCurrentSensitivity()
    {
        return PlayerPrefs.GetFloat(SensPlayerPrefKey, defaultSens);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
