using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using System.Linq;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapon Items")]
    [SerializeField] List<GunStats> gunList = new List<GunStats>();
    [SerializeField] float adsSpeed;
    [SerializeField] GameObject gunModel;
    [SerializeField] TMPro.TextMeshProUGUI ammoText;

    [Header("Weapon Components")]
    [SerializeField] AudioSource aud;
    [SerializeField] Camera gameplayCamera;

    [Header("VFX Prefabs")]
    [SerializeField] GameObject muzzleFlashPrefab;
    [SerializeField] GameObject tracerPrefab;
    [SerializeField] GameObject shellCasingPrefab;
    [SerializeField] float muzzleFlashTime;

    [Header("Layers")]
    [SerializeField] LayerMask ignoreLayer;

    [Header("Weapon Transforms")]
    [SerializeField] Transform barrelTip;
    [SerializeField] Transform shellEjectionPoint;
    [SerializeField] float shellEjectForce;
    [SerializeField] Transform leftHandGrip;
    [SerializeField] Transform rightHandGrip;

    [Header("Recoil Setup")]
    [SerializeField] private float weaponRecoilKick;
    [SerializeField] private float weaponRecoilRecoverySpeed;
    [SerializeField] float handRecoilKick;
    [SerializeField] float handRecoilRecoverySpeed;

    [Header("Animations")]
    [SerializeField] Animator animator;

    [Header("Runtime State")]
    Transform currentHipPosition;
    Transform currentAdsPosition;
    bool isAutomaticMode;
    bool isReloading;
    bool playedEmptySound;
    Coroutine reloadCoroutine;
    float shootCooldown;
    bool isAiming;


    private Vector3 initialGunPosition;
    private Vector3 currentGunOffset;
    private Vector3 initialLeftHandPos;
    private Vector3 initialRightHandPos;
    private Vector3 currentLeftHandOffset;
    private Vector3 currentRightHandOffset;

    private void Start()
    {

        if (gunModel != null)
        {
            initialGunPosition = gunModel.transform.localPosition;
        }

        if (leftHandGrip != null)
        {
            initialLeftHandPos = leftHandGrip.localPosition;
        }

        if (rightHandGrip != null)
        {
            initialRightHandPos = rightHandGrip.localPosition;
        }
        
        ammoText.SetText("00");
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
                ammoText.SetText(currentGun.ammoCur.ToString() + " / " + currentGun.ammoReserve.ToString() );
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
            //ammoText.SetText(currentGun.ammoCur.ToString());
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

        // RaycastAll returns all hits along the ray, including triggers
        RaycastHit[] hits = Physics.RaycastAll(ray, currentGun.shootRange, ~ignoreLayer, QueryTriggerInteraction.Ignore);

        // Sort hits by distance so we check closest first
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        RaycastHit? validHit = null;

        foreach (var hit in hits)
        {
            // Skip detection zones and lights zones by tag
            if (!hit.collider.CompareTag("Lights"))
            {
                validHit = hit;
                break;
            }
        }

        if (validHit.HasValue)
        {
            RaycastHit hit = validHit.Value;

            Debug.Log("Hit: " + hit.collider.name);

            // If enemy is hit, show enemy ui panel + enemy name
            if (hit.collider.CompareTag("Enemy"))
            {
                GameManager.instance.enemyInfoPanel.SetActive(true);
                GameManager.instance.enemyNameText.text = hit.collider.name;

            }

            if (currentGun.hitEffect != null)
            {
                var i = Instantiate(currentGun.hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }

            if (!hit.collider.CompareTag("Enemy") && currentGun.bulletHolePrefab != null)
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

            if (!hit.collider.CompareTag("Enemy") && tracerPrefab != null)//-----Tracer Prefab
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

            currentGunOffset.y -= weaponRecoilKick;

            //----- Apply hand recoil
            currentLeftHandOffset.z -= handRecoilKick;
            currentRightHandOffset.z -= handRecoilKick;

        }
    }

    IEnumerator ReloadRoutine(GunStats gun)//Handles reload and ammo limit reserve. 
    {
        isReloading = true;
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetTrigger("Reload");
        }

        if (gun.reloadSound != null) aud.PlayOneShot(gun.reloadSound, 0.8f);
        if (gun.reloadFreakingZombie != null) aud.PlayOneShot(gun.reloadFreakingZombie, 0.8f);
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
        ammoText.SetText(gun.ammoCur.ToString() + " / " + gun.ammoReserve.ToString());

    }

    public void AddAmmoToReserve(int ammoCount)
    {
        var gun = gunList[gunList.Count - 1];
        gun.ammoReserve += ammoCount;
        gun.ammoReserve = Mathf.Min(gun.ammoReserve + ammoCount, gun.maxAmmoReserve);
        StartCoroutine(AmmoFlash());
    }

    public void GetGunStats(GunStats gun)//---Gets gun and gunstats
    {
        gunList.Add(gun);
        isAutomaticMode = gun.isAutomaticDefault;

        gunModel.GetComponent<MeshFilter>().sharedMesh = gun.gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gun.gunModel.GetComponent<MeshRenderer>().sharedMaterial;

        currentHipPosition = gunModel.transform.Find("HipPosition");
        currentAdsPosition = gunModel.transform.Find("ADSPosition");
        ammoText.SetText(gun.ammoCur.ToString() + " / " + gun.ammoReserve.ToString());
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

        if (leftHandGrip != null)
        {
            leftHandGrip.localPosition = Vector3.Lerp(
                leftHandGrip.localPosition,
                initialLeftHandPos + currentLeftHandOffset,
                Time.deltaTime * handRecoilRecoverySpeed
            );
            currentLeftHandOffset = Vector3.Lerp(currentLeftHandOffset, Vector3.zero, Time.deltaTime * handRecoilRecoverySpeed);
        }

        if (rightHandGrip != null)
        {
            rightHandGrip.localPosition = Vector3.Lerp(
                rightHandGrip.localPosition,
                initialRightHandPos + currentRightHandOffset,
                Time.deltaTime * handRecoilRecoverySpeed
            );
            currentRightHandOffset = Vector3.Lerp(currentRightHandOffset, Vector3.zero, Time.deltaTime * handRecoilRecoverySpeed);
        }
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

    public GunStats CurrentGun
    {
        get
        {
            if (gunList == null || gunList.Count == 0)
                return null;
            return gunList[gunList.Count - 1];
        }
    }

    IEnumerator AmmoFlash()
    {
        GameManager.instance.flashAmmoPickUp.SetActive(true);
        CurrentGun.ammoReserve += 30;
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.flashAmmoPickUp.SetActive(false);
        ammoText.SetText(CurrentGun.ammoCur.ToString() + " / " + CurrentGun.ammoReserve.ToString());
    }

}
