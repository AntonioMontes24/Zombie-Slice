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

    bool isOpen = false; //toggle if door is open
    bool isLockedByEnemies;
    bool playerInRange;
    bool isAnimating = false;
    private Quaternion openRotation;
    private Quaternion closeRotation;
    private Quaternion targetRotation;
    private PlayerInventory playerInventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeRotation = doorHinge.transform.localRotation;
        openRotation = closeRotation * Quaternion.Euler(0, openAngleOffsetY, 0);
        targetRotation = closeRotation;
        if (doorType == DoorType.LockedUntilClear)
        {
            isLockedByEnemies = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(openCode) && playerInRange)
        {
            if (CanOpen())
            {
                ToggleDoor();
            }


        }
        if (Quaternion.Angle(doorHinge.transform.localRotation, targetRotation) > 0.01f)
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
        if (isOpen)
        {
            targetRotation = closeRotation * Quaternion.Euler(0, 90f, 0);
        }
        else
        {
            targetRotation = closeRotation;
        }
    }

    public void CloseDoor()
    {
        if (!isOpen)
        {
            return;
        }
        isOpen = false;
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
