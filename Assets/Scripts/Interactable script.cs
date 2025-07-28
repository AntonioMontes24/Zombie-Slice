using UnityEngine;
using TMPro;

public class PickupPromptTrigger : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI promptText; // Assign in Inspector
    [SerializeField] private string message = "Hold Interact to pick up";

    private void Start()
    {
        if (promptText != null)
            promptText.enabled = false; // Hide initially
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && promptText != null)
        {
            promptText.text = message;
            promptText.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && promptText != null)
        {
            promptText.enabled = false;
        }
    }
}