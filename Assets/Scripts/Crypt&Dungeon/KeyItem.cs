using UnityEngine;

public class KeyItem : MonoBehaviour
{
    public string keyID;

    private bool playerInRange = false;
    private PlayerInventory playerInventory;

    void Update()
    {
        if (playerInRange && PlayerController.inputActions.Input.Interact.triggered)
        {
            if (playerInventory != null)
            {
                playerInventory.AddKey(keyID);
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
