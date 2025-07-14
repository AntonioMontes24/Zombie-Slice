using UnityEngine;

public class DoorCloseTrigger : MonoBehaviour
{
    [SerializeField] doorScript[] doorsToClose;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var door in doorsToClose)
            {
                if (door != null)
                {
                    door.CloseDoor();
                }
            }
        }
    }
}
