using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerWeaponManager weaponManager;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] public GameObject armsModel;


    // Update is called once per frame
    void Update()
    {
        playerMovement.HandleMove();//Updates Movement Handling 
        playerMovement.HandleSprint();// Updates Sprint handling
        playerMovement.HandleLanding();// updates landing handling
        weaponManager.HandleShooting();// updates shooting

        if(armsModel != null)
        {
            armsModel.SetActive(weaponManager.HasGun());
        }

        if (Input.GetButtonDown("FireMode"))//Handles Firemode switch
        {
            weaponManager.ToggleFireMode();
        }
        weaponManager.HandleADS();// handles ads
        weaponManager.SetAiming(Input.GetButtonDown("Fire2"));// handles aiming
    }
}
