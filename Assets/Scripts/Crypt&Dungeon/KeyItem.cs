using UnityEngine;

public class KeyItem : MonoBehaviour
{
    public string keyID;

    private bool playerInRange = false;
    private PlayerInventory playerInventory;
    private PickupPromptTrigger pickupPrompt;

    private void Start()
    {
        pickupPrompt = GetComponent<PickupPromptTrigger>();
    }

    void Update()
    {
        if (playerInRange && PlayerController.inputActions.Input.Interact.triggered)
        {
            if (playerInventory != null)
            {
                playerInventory.AddKey(keyID);
                Destroy(gameObject);

                if (pickupPrompt != null && pickupPrompt.promptText != null)
                {
                    pickupPrompt.promptText.enabled = false;
                }

                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInventory = other.GetComponent<PlayerInventory>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInventory = null;
        }
    }
}
