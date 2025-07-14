using UnityEngine;
using UnityEngine.InputSystem;


namespace Crimson_Dusk.Managers
{
    public class InputManager : MonoBehaviour
    {

        public static InputManager Instance { get; private set; }

        [SerializeField] private InputActionAsset playerInputActions;

        private const string BINDING_OVERRIDES_KEY = "PlayerInputBindingOverrides";


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

    }

}
