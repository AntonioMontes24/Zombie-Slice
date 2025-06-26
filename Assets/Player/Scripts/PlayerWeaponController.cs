using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using System.Linq;
using UnityEngine.UI;
using NUnit.Framework;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapon Items")]
    [SerializeField] List<GunStats> gunList = new List<GunStats>();
    [SerializeField] float adsSpeed;
    [SerializeField] GameObject gunModel;
    [SerializeField] Transform weaponHolder;
    [SerializeField] TMPro.TextMeshProUGUI ammoText;

    [Header("Weapon Components")]
    [SerializeField] AudioSource aud;
    [SerializeField] Camera gameplayCamera;
    [SerializeField] public List<Image> hotBarSlots = new List<Image>();
    [SerializeField] int currentWeaponIndex;
    [SerializeField] List<Image> weaponIcons = new List<Image>();
    [SerializeField] List<Button> buttonHiglight = new List<Button>();
    private List<ColorBlock> originalButtonColors = new List<ColorBlock>();

    [Header("VFX Prefabs")]
    [SerializeField] GameObject muzzleFlashPrefab;
    [SerializeField] GameObject tracerPrefab;
    [SerializeField] GameObject shellCasingPrefab;
    [SerializeField] float muzzleFlashTime;

    [Header("Layers")]
    [SerializeField] LayerMask ignoreLayer;

    [Header("Weapon Transforms")]
    Transform barrelTip;
    Transform shellEjectionPoint;
    [SerializeField] float shellEjectForce;
    [SerializeField] Transform leftHandGrip;
    [SerializeField] Transform rightHandGrip;

    [Header("Recoil Setup")]
    [SerializeField] float weaponRecoilKick;
    [SerializeField] float weaponRecoilRecoverySpeed;
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
    GameObject currentWeaponInstance;

    private PlayerMovement movement;

    //GunRecoil and position
    private Vector3 currentGunOffset;
    private Vector3 initialLeftHandPos;
    private Vector3 initialRightHandPos;
    private Vector3 currentLeftHandOffset;
    private Vector3 currentRightHandOffset;

    private void Start()
    {
        if (leftHandGrip != null)
            initialLeftHandPos = leftHandGrip.localPosition;

        if (rightHandGrip != null)
            initialRightHandPos = rightHandGrip.localPosition;

        ammoText.SetText("00");

        movement = GetComponentInParent<PlayerMovement>();
        if (movement == null)
            Debug.LogWarning("Player movement not found in parent");

        originalButtonColors.Clear();// Grabs original HotBar color
        foreach (var bnt in buttonHiglight)
        {
            if (bnt != null)
                originalButtonColors.Add(bnt.colors);
            else
                originalButtonColors.Add(default);
        }
    }

    public void GetGunStats(GunStats gun, int startingAmmo = -1, int reserveAmmo = -1)// Optional Parameters to modify starting ammo 
    {
        GunStats runtimeGun = gun.Clone();

        runtimeGun.ammoCur = (startingAmmo >= 0) ? startingAmmo : runtimeGun.ammoMax;
        runtimeGun.ammoReserve = (reserveAmmo >= 0) ? reserveAmmo : runtimeGun.maxAmmoReserve;

        gunList.Add(runtimeGun);
        EquipWeapon(gunList.Count - 1);
    }


    public void HandleShooting()//Handles Shooting
    {
        if (movement != null && movement.canSprint && Input.GetButton("Sprint"))
            return;

        shootCooldown -= Time.deltaTime;
        if (gunList.Count == 0) return;
        GunStats currentGun = gunList[currentWeaponIndex];
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
                    reloadCoroutine = StartCoroutine(ReloadRoutine(currentGun));
            }
            else
            {
                if (currentGun.ammoReserve > 0 && reloadCoroutine == null) // Checks ammo Reserve
                    reloadCoroutine = StartCoroutine(ReloadRoutine(currentGun));
                else if (currentGun.emptySound != null && !playedEmptySound)// Flag to avoid empty sound spam
                {
                    aud.PlayOneShot(currentGun.emptySound);
                    playedEmptySound = true;
                }
            }
        }

        if (!Input.GetButton("Fire1"))
            playedEmptySound = false;

        if (Input.GetKeyDown(KeyCode.R) && currentGun.ammoCur < currentGun.ammoMax && currentGun.ammoReserve > 0 && !isReloading)
            reloadCoroutine = StartCoroutine(ReloadRoutine(currentGun));// Starts Reload
        //ammoText.SetText(currentGun.ammoCur.ToString());
    }

    void Shoot()//Handles damage/Ray cast/ and checks for current gun and gun stats
    {
        if (gunList.Count == 0 || gameplayCamera == null) return;
        GunStats currentGun = gunList[currentWeaponIndex];

        if (currentGun.shootSound != null)
            aud.PlayOneShot(currentGun.shootSound, currentGun.shootVol);

        Ray ray;
        if (isAiming)
            ray = gameplayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
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

        
        if (muzzleFlashPrefab != null && barrelTip != null)//Muzzle flash handler
        {
            muzzleFlashTime = 0.1f;
            GameObject flash = Instantiate(muzzleFlashPrefab, barrelTip.position, barrelTip.rotation, barrelTip);
            Destroy(flash, muzzleFlashTime);
        }

        Debug.DrawRay(ray.origin, ray.direction * currentGun.shootRange, Color.red, 1f);
        RaycastHit[] hits = Physics.RaycastAll(ray, currentGun.shootRange, ~ignoreLayer, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        RaycastHit? validHit = hits.FirstOrDefault(hit => !hit.collider.CompareTag("Lights") && !hit.collider.CompareTag("Player"));

        if (validHit.HasValue)
        {
            RaycastHit hit = validHit.Value;

            iEnemyHealth enemyHealth = hit.collider.GetComponent<iEnemyHealth>();
            IDamage dmg = hit.collider.GetComponent<IDamage>();


            if (hit.collider.CompareTag("Enemy") && enemyHealth != null)
            {

                GameManager.instance.SetCurrentEnemy(enemyHealth);
            
            } else
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    Debug.LogWarning("Enemy hit, but iEnemyHealth component not found on: " + hit.collider.name);
                }
                GameManager.instance.HideEnemyUI();
            }

            if (currentGun.hitEffect != null)
                Instantiate(currentGun.hitEffect, hit.point, Quaternion.LookRotation(hit.normal));

            if (!hit.collider.CompareTag("Enemy") && currentGun.bulletHolePrefab != null)
            {
                var bulletHole = Instantiate(currentGun.bulletHolePrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
                bulletHole.transform.SetParent(hit.transform);
                Destroy(bulletHole, 10f);
            }
            else if (hit.collider.CompareTag("Enemy") && currentGun.zombieBloodHit != null)
            {
                var bloodEffect = Instantiate(currentGun.zombieBloodHit, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
                bloodEffect.transform.SetParent(hit.transform);
                Destroy(bloodEffect, 0.5f);
            }

            
            if (dmg != null)
                dmg.takeDamage(currentGun.shootDamage);

            if (!hit.collider.CompareTag("Enemy") && tracerPrefab != null)
            {
                GameObject tracer = Instantiate(tracerPrefab, barrelTip.position, Quaternion.LookRotation(hit.point - barrelTip.position));
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
            currentLeftHandOffset.z -= handRecoilKick;
            currentRightHandOffset.z -= handRecoilKick;
        } else
        {
            GameManager.instance.HideEnemyUI();
        }
    }

    IEnumerator ReloadRoutine(GunStats gun)//Handles reload and ammo limit reserve. 
    {
        isReloading = true;
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetTrigger("Reload");

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

    public void SetAiming(bool aim)//Sets aiming bool
    {
        isAiming = aim;
        if (animator != null)
        {
            animator.SetBool("IsAiming", isAiming);
        }
    }

    public void HandleADS()//Handles ads position/recoil
    {
        if (currentHipPosition == null || currentAdsPosition == null)
            return;

        Transform target = isAiming ? currentAdsPosition : currentHipPosition;

        Vector3 recoilAdjustedPosition = target.localPosition + currentGunOffset;
        currentWeaponInstance.transform.localPosition = Vector3.Lerp(
            currentWeaponInstance.transform.localPosition,
            recoilAdjustedPosition,
            Time.deltaTime * adsSpeed
        );

        currentGunOffset = Vector3.Lerp(currentGunOffset, Vector3.zero, Time.deltaTime * weaponRecoilRecoverySpeed);
        currentWeaponInstance.transform.localRotation = Quaternion.Slerp(
            currentWeaponInstance.transform.localRotation,
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
        GunStats currentGun = gunList[currentWeaponIndex];

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

    public GunStats CurrentGun
    {
        get
        {
            if (gunList == null || gunList.Count == 0)
                return null;
            return gunList[currentWeaponIndex];
        }
    }

    public void AddAmmoToReserve(int ammoCount)
    {
        var gun = gunList[currentWeaponIndex];
        gun.ammoReserve += ammoCount;
        gun.ammoReserve = Mathf.Min(gun.ammoReserve, gun.maxAmmoReserve);
        StartCoroutine(AmmoFlash());
    }

    IEnumerator AmmoFlash()
    {
        GameManager.instance.flashAmmoPickUp.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.flashAmmoPickUp.SetActive(false);
        ammoText.SetText(CurrentGun.ammoCur.ToString() + " / " + CurrentGun.ammoReserve.ToString());
    }

    public void UpdateAmmoUi()// Helper to update Ammo UI
    {
        if (HasGun())
        {
            GunStats gun = CurrentGun;
            ammoText.SetText(gun.ammoCur + " / " + gun.ammoReserve);
        }
    }

    public void EquipWeapon(int index)// Equips weapon and assings Gun icon
    {
        if (index < 0 || index >= gunList.Count)
        {
            Debug.LogWarning("EquipWeapon: Invalid index");
            return;
        }

        currentWeaponIndex = index;
        GunStats gun = gunList[currentWeaponIndex];
        isAutomaticMode = gun.isAutomaticDefault;

        if (currentWeaponInstance != null)
            Destroy(currentWeaponInstance);

        currentWeaponInstance = Instantiate(gun.gunModel, weaponHolder);
        currentWeaponInstance.transform.localPosition = Vector3.zero;
        currentWeaponInstance.transform.localRotation = Quaternion.identity;

        currentHipPosition = currentWeaponInstance.transform.Find("HipPosition");
        currentAdsPosition = currentWeaponInstance.transform.Find("ADSPosition");
        barrelTip = currentWeaponInstance.transform.Find("BarrelTip");
        shellEjectionPoint = currentWeaponInstance.transform.Find("ShellEjection");

        ammoText.SetText(gun.ammoCur + " / " + gun.ammoReserve);

        for (int i = 0; i < weaponIcons.Count; i++)
        {
            bool hasWeapon = i < gunList.Count && gunList[i].gunIcon != null;

            if (weaponIcons[i] != null)
            {
                weaponIcons[i].enabled = hasWeapon;

                if (hasWeapon)
                {
                    weaponIcons[i].sprite = gunList[i].gunIcon;
                    weaponIcons[i].color = Color.white;
                }
            }

            if (buttonHiglight != null && i < buttonHiglight.Count && buttonHiglight[i] != null)
            {
                ColorBlock cb = originalButtonColors[i];
                cb.normalColor = (i == index) ? Color.white : originalButtonColors[i].normalColor;
                cb.highlightedColor = (i == index) ? Color.white : originalButtonColors[i].highlightedColor;
                cb.selectedColor = cb.normalColor;
                cb.pressedColor = originalButtonColors[i].pressedColor;
                buttonHiglight[i].colors = cb;

                buttonHiglight[i].gameObject.SetActive(hasWeapon);
            }
        }
    }


    public void ScrollWeapon(int direction)// Scroll weapon selection 
    {
        if (gunList.Count == 0) return;

        int newIndex = (currentWeaponIndex + direction + gunList.Count) % gunList.Count;

        if (newIndex == currentWeaponIndex)
            return; // Don't re-equip the same weapon

        EquipWeapon(newIndex);
    }

    public void TryEquipWeapon(int index)
    {
        if (index < 0 || index >= gunList.Count) return;
        if (index == currentWeaponIndex) return;

        EquipWeapon(index);
    }

}
