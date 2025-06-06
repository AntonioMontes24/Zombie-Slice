using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerWeaponManager weaponManager;


    // Update is called once per frame
    void Update()
    {
        playerMovement.HandleMove();
        playerMovement.HandleSprint();
        playerMovement.HandleLanding();
        weaponManager.HandleShooting();

        if (Input.GetButtonDown("FireMode"))
        {
            weaponManager.ToggleFireMode();
        }
        weaponManager.HandleADS();
        weaponManager.SetAiming(Input.GetButtonDown("Fire2"));
    }
}
