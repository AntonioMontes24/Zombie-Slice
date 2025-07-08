using UnityEngine;

public class doorScript : MonoBehaviour
{
    [SerializeField] KeyCode openCode = KeyCode.E; //Letter to press to interact with door
    [SerializeField] float openSpeed;  //Adjust open speed of door
    [SerializeField] public GameObject doorHinge;
    bool isOpen = false; //toggle if door is open
    bool isRotating;
    bool hasKey; //If door requires key toggle.
    /*The has key bool will need to interact with the playerscript in someway to see if the key is active and present on the player.*/
    bool playerInRange;
    private Quaternion openRotation;
    private Quaternion closeRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeRotation = transform.rotation;
        openRotation = closeRotation * Quaternion.Euler(0, 90f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(openCode) && playerInRange)
        {
            if (isOpen)
            {
                doorHinge.transform.Rotate(0f, 90f, 0f);
                isOpen = false;
            }
            else if (!isOpen)
            {
                doorHinge.transform.Rotate(0f, -90f, 0f);
                isOpen = true;
            }
            
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
        }
    }
}
