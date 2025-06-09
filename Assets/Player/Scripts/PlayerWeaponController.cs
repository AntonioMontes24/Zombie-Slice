using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;

public class PlayerWeaponManager : MonoBehaviour
{
    [SerializeField] List<GunStats> gunList = new List<GunStats>();
    [SerializeField] AudioSource aud;
    [SerializeField] GameObject muzzleFlashPrefab;
    [SerializeField] float muzzleFlashTime;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] GameObject gunModel;
    [SerializeField] float adsSpeed;
    [SerializeField] GameObject tracerPrefab;
    [SerializeField] Transform barrelTip;
    [SerializeField] Camera gameplayCamera;
    [SerializeField] GameObject shellCasingPrefab;
    [SerializeField] Transform shellEjectionPoint;
    [SerializeField] float shellEjectForce;


    //ADS and Hip position transforms
    Transform currentHipPosition;
    Transform currentAdsPosition;

    bool isAutomaticMode;
    bool isReloading;
    bool playedEmptySound;
    Coroutine reloadCoroutine;
    float shootCooldown;
    bool isAiming;
    int currentAmmoLimit;

    [SerializeField] private float weaponRecoilKick;
    [SerializeField] private float weaponRecoilRecoverySpeed;

    private Vector3 initialGunPosition;
    private Vector3 currentGunOffset;

    private void Start()
    {
        if (gunModel != null)
        {
            initialGunPosition = gunModel.transform.localPosition;
        }
    }

    public void HandleShooting()//Handles Shooting
    {
        shootCooldown -= Time.deltaTime;
        if (gunList.Count == 0) return;
        GunStats currentGun = gunList[gunList.Count - 1];

        if (isReloading) return;

        bool fireInput = isAutomaticMode ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        if (fireInput && shootCooldown <= 0f)
        {
            if (currentGun.ammoCur > 0) // Check if mag is not empty, then fire
            {
                shootCooldown = isAutomaticMode ? currentGun.autoFireRate : currentGun.semiFireRate;
                Shoot();
                currentGun.ammoCur--;
                playedEmptySound = false;

                if (currentGun.ammoCur <= 0 && currentGun.ammoReserve > 0)
                {
                    reloadCoroutine = StartCoroutine(ReloadRoutine(currentGun));
                }
            }
            else
            {
                if (currentGun.ammoReserve > 0 && reloadCoroutine == null) // Checks ammo Reserve
                {
                    reloadCoroutine = StartCoroutine(ReloadRoutine(currentGun));
                    playedEmptySound = false;
                }
                else if (currentGun.emptySound != null && !playedEmptySound)// Flag to avoid empty sound spam 
                {
                    aud.PlayOneShot(currentGun.emptySound);
                    playedEmptySound = true;
                }
            }
        }

        // Reset the empty sound flag when player releases fire
        if (!Input.GetButton("Fire1"))
        {
            playedEmptySound = false;
        }

        if (Input.GetKeyDown(KeyCode.R) && currentGun.ammoCur < currentGun.ammoMax && currentGun.ammoReserve > 0 && !isReloading)
        {
            reloadCoroutine = StartCoroutine(ReloadRoutine(currentGun));// Starts Reload
        }
    }


    void Shoot()//Handles damage/Ray cast/ and checks for current gun and gun stats
    {
        if (gunList.Count == 0 || gameplayCamera == null) return;
        GunStats currentGun = gunList[gunList.Count - 1];

        if (currentGun.shootSound != null)
            aud.PlayOneShot(currentGun.shootSound, currentGun.shootVol);

        Ray ray;

        if (isAiming)
        {
            ray = gameplayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }
        else
        {
            float spreadAngle = 5f;
            Vector3 spreadDir = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0
            ) * barrelTip.forward;

            ray = new Ray(barrelTip.position, spreadDir);
        }

        if (muzzleFlashPrefab != null)//Muzzle flash handler
            StartCoroutine(MuzzleFlashRoutine());

        Debug.DrawRay(ray.origin, ray.direction * currentGun.shootRange, Color.red, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, currentGun.shootRange, ~ignoreLayer))
        {
            Debug.Log("Hit: " + hit.collider.name);// Debug screen message to check what was hit
            if (currentGun.hitEffect != null)
            {
                var i = Instantiate(currentGun.hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(i, 2f);
            }

            if (currentGun.bulletHolePrefab != null)
            {
                var bulletHole = Instantiate(
                    currentGun.bulletHolePrefab,
                    hit.point + hit.normal * 0.01f,
                    Quaternion.LookRotation(hit.normal)
                );
                bulletHole.transform.SetParent(hit.transform);
                Destroy(bulletHole, 10f);
            }

            IDamage dmg = hit.collider.GetComponent<IDamage>(); //------Damage
            if (dmg != null)
                dmg.takeDamage(currentGun.shootDamage);

            if (tracerPrefab != null)//-----Tracer Prefab
            {
                Vector3 tracerStart = barrelTip.position;
                GameObject tracer = Instantiate(tracerPrefab, tracerStart, Quaternion.identity);
                tracer.transform.LookAt(hit.point);
                Rigidbody rb = tracer.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddForce(tracer.transform.forward * 1000f, ForceMode.Impulse);
                Destroy(tracer, 2f);
            }

            if (shellCasingPrefab != null && shellEjectionPoint != null)//------Shell casing Prefab
            {
                GameObject shell = Instantiate(shellCasingPrefab, shellEjectionPoint.position, shellEjectionPoint.rotation);
                Rigidbody rb = shell.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 ejectDirection = shellEjectionPoint.right + // Random Shell ejection
                                             (shellEjectionPoint.up * Random.Range(-0.2f, 0.2f)) +
                                             (shellEjectionPoint.forward * Random.Range(-0.1f, 0.1f));

                    rb.AddForce(ejectDirection.normalized * shellEjectForce, ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * shellEjectForce, ForceMode.Impulse);
                }
                Destroy(shell, 3f);
            }
            currentGunOffset.z -= weaponRecoilKick;
        }
    }

    IEnumerator ReloadRoutine(GunStats gun)//Handles reload and ammo limit reserve. 
    {
        isReloading = true;
        if (gun.reloadSound != null) aud.PlayOneShot(gun.reloadSound, 0.8f);
        yield return new WaitForSeconds(gun.reloadTime);

        int needed = gun.ammoMax - gun.ammoCur;

        if(gun.ammoReserve >= needed)
        {
            gun.ammoCur += needed;
            gun.ammoReserve -= needed;
        }
        else
        {
            gun.ammoCur += gun.ammoReserve;
            gun.ammoReserve = 0;
        }
        isReloading = false;
        reloadCoroutine = null;
    }

    public void GetGunStats(GunStats gun)//---Gets gun and gunstats
    {
        gunList.Add(gun);
        isAutomaticMode = gun.isAutomaticDefault;

        gunModel.GetComponent<MeshFilter>().sharedMesh = gun.gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gun.gunModel.GetComponent<MeshRenderer>().sharedMaterial;

        currentHipPosition = gunModel.transform.Find("HipPosition");
        currentAdsPosition = gunModel.transform.Find("ADSPosition");
    }

    public void SetAiming(bool aim)//Sets aiming bool
    {
        isAiming = aim;
    }

    public void HandleADS()//Handles ads position/recoil
    {
        if (currentHipPosition == null || currentAdsPosition == null)
            return;

        Transform target = isAiming ? currentAdsPosition : currentHipPosition;

        Vector3 recoilAdjustedPosition = target.localPosition + currentGunOffset;
        gunModel.transform.localPosition = Vector3.Lerp(
            gunModel.transform.localPosition,
            recoilAdjustedPosition,
            Time.deltaTime * adsSpeed
        );

        currentGunOffset = Vector3.Lerp(currentGunOffset, Vector3.zero, Time.deltaTime * weaponRecoilRecoverySpeed);
        gunModel.transform.localRotation = Quaternion.Slerp(
            gunModel.transform.localRotation,
            target.localRotation,
            Time.deltaTime * adsSpeed
        );
    }

    public void ToggleFireMode()//Sets Firemode
    {
        if (gunList.Count == 0) return;
        GunStats currentGun = gunList[gunList.Count - 1];

        if (currentGun.canSwitchFireMode)
        {
            isAutomaticMode = !isAutomaticMode;

            if (currentGun.fireModeSwitchSound != null)
                aud.PlayOneShot(currentGun.fireModeSwitchSound, 0.6f);
        }
    }

    public bool HasGun()//Checks if there is a current gun 
    {
        return gunList.Count > 0;
    }

    IEnumerator MuzzleFlashRoutine()//----Muzzle Flash vfx handler
    {
        muzzleFlashPrefab.SetActive(true);
        yield return new WaitForSeconds(muzzleFlashTime);
        muzzleFlashPrefab.SetActive(false);
    }

    void OnGUI()//---- TEMP UI 
    {
        if (gunList.Count == 0) return;
        GunStats currentGun = gunList[gunList.Count - 1];

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.normal.textColor = Color.blueViolet;

        GUI.Label(new Rect(300, 10, 300, 40), "Ammo: " + currentGun.ammoCur + " / " + currentGun.ammoMax, style);
        GUI.Label(new Rect(1000,10,300,40), "Ammo Reserve: " + currentGun.ammoReserve,style);
    }
}
