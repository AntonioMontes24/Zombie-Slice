using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ControllerSensController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider controllerSlider;
    [SerializeField] private TMP_InputField controllerInputField;
    [SerializeField] private Button controllerResetButton;

    [Header("Settings")]
    [SerializeField] private float defaultControllerSens = 50.0f;
    [SerializeField] private float minControllerSens = 10.0f;
    [SerializeField] private float maxControllerSens = 200.0f;

    private const string ControllerSensKey = "ControllerSensitivity";

    public static event Action<float> OnControllerSensChanged;

    private void Awake()
    {
        if (controllerSlider == null) Debug.LogError("Controller slider not assigned!!");
        if (controllerInputField == null) Debug.LogError("Controller input box not assigned!!");
        if (controllerResetButton == null) Debug.LogError("Controller reset button not assigned!!");
    }

    void Start()
    {
        float loadedSens = PlayerPrefs.GetFloat(ControllerSensKey, defaultControllerSens);
        loadedSens = Mathf.Clamp(loadedSens, minControllerSens, maxControllerSens);

        controllerSlider.minValue = minControllerSens;
        controllerSlider.maxValue = maxControllerSens;
        UpdateUI(loadedSens);

        controllerSlider.onValueChanged.AddListener(OnSliderValueChanged);
        controllerInputField.onEndEdit.AddListener(OnInputFieldValueChanged);
        controllerResetButton.onClick.AddListener(ResetSensitivity);
    }

    private void OnDestroy()
    {
        controllerSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        controllerInputField.onEndEdit.RemoveListener(OnInputFieldValueChanged);
        controllerResetButton.onClick.RemoveListener(ResetSensitivity);
    }

    private void UpdateUI(float newSens)
    {
        controllerSlider.value = newSens;
        controllerInputField.text = newSens.ToString("F1");
        PlayerPrefs.SetFloat(ControllerSensKey, newSens);
        PlayerPrefs.Save();
        OnControllerSensChanged?.Invoke(newSens);
        Debug.Log($"Controller Sens changed to: {newSens}.");
    }

    private void OnSliderValueChanged(float value)
    {
        UpdateUI(value);
    }

    private void OnInputFieldValueChanged(string text)
    {
        if (float.TryParse(text, out float parsedSensitivity))
        {
            parsedSensitivity = Mathf.Clamp(parsedSensitivity, minControllerSens, maxControllerSens);
            UpdateUI(parsedSensitivity);
        }
        else
        {
            controllerInputField.text = controllerSlider.value.ToString("F1");
            Debug.LogWarning("Invalid input for controller sens. Please enter a number.");
        }
    }

    public void ResetSensitivity()
    {
        UpdateUI(defaultControllerSens);
    }

    public float GetCurrentSensitivity()
    {
        return PlayerPrefs.GetFloat(ControllerSensKey, defaultControllerSens);
    }

    void Update()
    {

    }
}
