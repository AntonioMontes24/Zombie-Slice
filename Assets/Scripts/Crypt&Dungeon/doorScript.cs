using System.Collections;
using UnityEngine;

public class doorScript : MonoBehaviour
{
    public enum DoorType { Normal, RequiresKey, LockedUntilClear }
    [SerializeField] private DoorType doorType = DoorType.Normal;
    [SerializeField] KeyCode openCode = KeyCode.F; //Letter to press to interact with door
    [SerializeField] float openSpeed;  //Adjust open speed of door
    [SerializeField] string requiredKeyID; //For doors that require a key
    [SerializeField] EnemyRoom enemyRoom;
    [SerializeField] public GameObject doorHinge;
    [SerializeField] float openAngleOffsetY = -90f;
    [SerializeField] bool useSlidingDoor = false; //sliding door 
    [SerializeField] Vector3 openPositionOffSet = new Vector3(0, 0, -3f); //door sliding
    [SerializeField] private bool flipOpenDirection = false;

    private Vector3 closedPosition;
    private Vector3 targetPosition;

    bool isOpen = false; //toggle if door is open
    bool isLockedByEnemies;
    bool playerInRange;
    bool isAnimating = false;
    private Quaternion openRotation;
    private Quaternion closeRotation;
    private Quaternion targetRotation;
    private PlayerInventory playerInventory;


    //trying to make door not hit player upon open

    [SerializeField] private Collider doorCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        closedPosition = doorHinge.transform.localPosition; //starting POS
        targetPosition = closedPosition;

        closeRotation = doorHinge.transform.localRotation;
        openRotation = closeRotation * Quaternion.Euler(0, openAngleOffsetY, 0);
        targetRotation = closeRotation;
        if (doorType == DoorType.LockedUntilClear)
        {
            isLockedByEnemies = true;
        }

        if (doorCollider == null)
        {
            doorCollider = doorHinge.GetComponent<Collider>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerController.inputActions.Input.Interact.triggered && playerInRange)
        {
            if (CanOpen())
            {
                ToggleDoor();
            }
        }

        if (useSlidingDoor)
        {
            doorHinge.transform.localPosition = Vector3.Lerp(doorHinge.transform.localPosition, targetPosition, Time.deltaTime * openSpeed);
        }

        else if (Quaternion.Angle(doorHinge.transform.localRotation, targetRotation) > 0.01f)
        {
            doorHinge.transform.localRotation = Quaternion.Slerp(doorHinge.transform.localRotation, targetRotation, Time.deltaTime * openSpeed);
        }
    }

    private bool CanOpen()
    {
        switch (doorType)
        {
            case DoorType.Normal:
                return true;
            case DoorType.RequiresKey:
                if (playerInventory != null && playerInventory.HasKey(requiredKeyID))
                {
                    return true;
                }
                return false;
            case DoorType.LockedUntilClear:
                if (enemyRoom != null)
                {
                    if (!enemyRoom.RoomTriggered)
                    {
                        return true;
                    }
                    return enemyRoom.AreAllEnemiesDefeated();
                }
                return false;

            default:
                return false;
        }
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;

        if (doorCollider != null)
            doorCollider.enabled = false; // Disable collider before rotation

        if (useSlidingDoor)
        {
            targetPosition = isOpen ? closedPosition + openPositionOffSet : closedPosition;

        }


        else if (isOpen)
        {
            float angle = flipOpenDirection ? -openAngleOffsetY : openAngleOffsetY;
            targetRotation = closeRotation * Quaternion.Euler(0, angle, 0);
        }
        else
        {
            targetRotation = closeRotation;
        }

        StartCoroutine(ReenableColliderAfterDelay(.5f)); // Adjustable delay
    }

    private IEnumerator ReenableColliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (doorCollider != null)
            doorCollider.enabled = true;
    }

    public void CloseDoor()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;

        if (useSlidingDoor)
            targetPosition = closedPosition;
        else
            targetRotation = closeRotation;
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
        }
    }

    public void UnlockDoor()
    {
        isLockedByEnemies = false;
    }

    public void LockDoorByEnemies()
    {
        if (doorType == DoorType.LockedUntilClear)
        {
            isLockedByEnemies = true;
        }
    }

}
