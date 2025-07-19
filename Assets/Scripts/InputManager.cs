using UnityEngine;
using UnityEngine.InputSystem;

namespace Crimson_Dusk.Managers
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [SerializeField] private InputActionAsset playerInputActions;

        private const string BINDING_OVERRIDES_KEY = "PlayerInputBindingOverrides";
        private const string MOUSE_SENS_KEY = "MouseSensitivity";
        private const string CONTROLLER_SENS_KEY = "ControllerSensitivity";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadAllBindingOverrides();
            playerInputActions.Enable();

            if (!PlayerPrefs.HasKey(MOUSE_SENS_KEY))
                SetMouseSensitivity(3.5f);

            if (!PlayerPrefs.HasKey(CONTROLLER_SENS_KEY))
                SetControllerSensitivity(7.5f);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                playerInputActions.Disable();
                Instance = null;
            }
        }

        public InputActionAsset GetPlayerInputActions()
        {
            return playerInputActions;
        }

        // Binding overrides
        public void SaveAllBindingOverrides()
        {
            string overrides = playerInputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(BINDING_OVERRIDES_KEY, overrides);
            PlayerPrefs.Save();
            Debug.Log("Saved all binding overrides");
        }

        public void LoadAllBindingOverrides()
        {
            if (PlayerPrefs.HasKey(BINDING_OVERRIDES_KEY))
            {
                string overrides = PlayerPrefs.GetString(BINDING_OVERRIDES_KEY);
                if (!string.IsNullOrEmpty(overrides))
                {
                    playerInputActions.LoadBindingOverridesFromJson(overrides);
                    Debug.Log("Loaded all binding overrides");
                }
            }
        }

        public void ResetAllBindings()
        {
            playerInputActions.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey(BINDING_OVERRIDES_KEY);
            PlayerPrefs.Save();
            Debug.Log("Reset all bindings to default.");
        }

        // Sensitivity: Mouse
        public float GetMouseSensitivity()
        {
            return PlayerPrefs.GetFloat(MOUSE_SENS_KEY, 3.5f); // default fallback
        }

        public void SetMouseSensitivity(float value)
        {
            PlayerPrefs.SetFloat(MOUSE_SENS_KEY, value);
            PlayerPrefs.Save();
            Debug.Log($"Saved Mouse Sensitivity: {value}");
        }

        // Sensitivity: Controller
        public float GetControllerSensitivity()
        {
            return PlayerPrefs.GetFloat(CONTROLLER_SENS_KEY, 7.5f); // default fallback
        }

        public void SetControllerSensitivity(float value)
        {
            PlayerPrefs.SetFloat(CONTROLLER_SENS_KEY, value);
            PlayerPrefs.Save();
            Debug.Log($"Saved Controller Sensitivity: {value}");
        }
    }
}
