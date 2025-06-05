using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;


    // Update is called once per frame
    void Update()
    {
        playerMovement.HandleMove();
        playerMovement.HandleSprint();
        playerMovement.HandleLanding();
    }
}
