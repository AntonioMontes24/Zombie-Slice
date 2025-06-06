using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;

public class PlayerWeaponManager : MonoBehaviour
{
    //Gunstats
    public List<GunStats> gunList = new List<GunStats>();
    public AudioSource aud;
    public GameObject muzzleFlashPrefab;
    public float muzzleFlashTime = 0.05f;
    public LayerMask ignoreLayer;
    public GameObject gunModel;
    public float adsSpeed;
    public GameObject tracerPrefab;
    public Transform barrelTip;

    //Camera
    public Camera gameplayCamera;

    //ShellCasing
    public GameObject shellCasingPrefab;
    public Transform shellEjectionPoint;
    public float shellEjectForce = 2f;

    //ADS
    Transform currentHipPosition;
    Transform currentAdsPosition;
    //Firemode
    bool isAutomaticMode;
    bool isReloading;
    Coroutine reloadCoroutine;
    float shootCooldown = 0f;
    bool isAiming;

    //Recoil
    public float weaponRecoilKick = 0.1f;
    public float weaponRecoilRecoverySpeed = 10f;

    private Vector3 initialGunPosition;
    private Vector3 currentGunOffset;



    private void Start()
    {
        if(gunModel != null)
        {
            initialGunPosition = gunModel.transform.localPosition;
        }
    }


    public void HandleShooting()
    {
        shootCooldown -= Time.deltaTime;
        if (gunList.Count == 0) return;
        GunStats currentGun = gunList[gunList.Count - 1];

        if (isReloading) return;

        bool fireInput = isAutomaticMode ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        if (fireInput && shootCooldown <= 0f)
        {
            if (currentGun.ammoCur > 0)
            {
                shootCooldown = isAutomaticMode ? currentGun.autoFireRate : currentGun.semiFireRate;
                Shoot();
                currentGun.ammoCur--;

                if (currentGun.ammoCur <= 0)
                    reloadCoroutine = StartCoroutine(ReloadRoutine(currentGun));
            }
            else
            {
                if (reloadCoroutine == null) reloadCoroutine = StartCoroutine(ReloadRoutine(currentGun));
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && currentGun.ammoCur < currentGun.ammoMax && !isReloading)
            reloadCoroutine = StartCoroutine(ReloadRoutine(currentGun));
    }

    void Shoot()
    {
        if (gunList.Count == 0 || gameplayCamera == null) return;
        GunStats currentGun = gunList[gunList.Count - 1];

        if (currentGun.shootSound != null)
            aud.PlayOneShot(currentGun.shootSound, currentGun.shootVol);

        Ray ray = gameplayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (muzzleFlashPrefab != null)
            StartCoroutine(MuzzleFlashRoutine());

        Debug.DrawRay(ray.origin, ray.direction * currentGun.shootRange, Color.red, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, currentGun.shootRange, ~ignoreLayer))
        {
            Debug.Log("Hit: " + hit.collider.name);

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

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
                dmg.takeDamage(currentGun.shootDamage);

            if (tracerPrefab != null && barrelTip != null)
            {
                GameObject tracer = Instantiate(tracerPrefab, barrelTip.position, barrelTip.rotation);
                tracer.transform.LookAt(hit.point);
                Rigidbody rb = tracer.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddForce(tracer.transform.forward * 1000f);
                Destroy(tracer, 2f);

            }

            if (shellCasingPrefab != null && shellEjectionPoint != null)
            {
                GameObject shell = Instantiate(shellCasingPrefab, shellEjectionPoint.position, shellEjectionPoint.rotation);
                Rigidbody rb = shell.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(shellEjectionPoint.forward * shellEjectForce, ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * shellEjectForce, ForceMode.Impulse);
                }
                Destroy(shell, 3f);
            }

            currentGunOffset.z -= weaponRecoilKick;

        }
    }

    IEnumerator ReloadRoutine(GunStats gun)
    {
        isReloading = true;
        if (gun.reloadSound != null) aud.PlayOneShot(gun.reloadSound, 0.8f);
        yield return new WaitForSeconds(gun.reloadTime);
        gun.ammoCur = gun.ammoMax;
        isReloading = false;
        reloadCoroutine = null;
    }

    public void GetGunStats(GunStats gun)
    {
        gunList.Add(gun);
        isAutomaticMode = gun.isAutomaticDefault;

        gunModel.GetComponent<MeshFilter>().sharedMesh = gun.gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gun.gunModel.GetComponent<MeshRenderer>().sharedMaterial;

        currentHipPosition = gunModel.transform.Find("HipPosition");
        currentAdsPosition = gunModel.transform.Find("ADSPosition");
    }

    public void SetAiming(bool aim)
    {
        isAiming = aim;
    }

    public void HandleADS()
    {
        if (currentHipPosition == null || currentAdsPosition == null)
            return;

        Transform target = isAiming ? currentAdsPosition : currentHipPosition;

        Vector3 recoilAdjustedPosition = target.localPosition + currentGunOffset;
        gunModel.transform.localPosition = Vector3.Lerp
            (
            gunModel.transform.localPosition,
            recoilAdjustedPosition,
            Time.deltaTime * adsSpeed
        );

        currentGunOffset = Vector3.Lerp(currentGunOffset, Vector3.zero, Time.deltaTime * weaponRecoilRecoverySpeed);
        gunModel.transform.localRotation = Quaternion.Slerp
            (
            gunModel.transform.localRotation,
            target.localRotation,
            Time.deltaTime * adsSpeed
        );
    }

    public void ToggleFireMode()
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

    public bool HasGun()
    {
        return gunList.Count > 0;
    }

    IEnumerator MuzzleFlashRoutine()
    {
        muzzleFlashPrefab.SetActive(true);
        yield return new WaitForSeconds(muzzleFlashTime);
        muzzleFlashPrefab.SetActive(false);
    }

}
