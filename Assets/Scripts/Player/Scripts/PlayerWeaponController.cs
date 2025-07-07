using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using System.Linq;
using UnityEngine.UI;
using NUnit.Framework;
using UnityEngine.Audio;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapon Items")]
    [SerializeField] List<WeaponStats> weaponList = new List<WeaponStats>();
    [SerializeField] float adsSpeed;
    [SerializeField] GameObject gunModel;
    [SerializeField] Transform weaponHolder;
    [SerializeField] TMPro.TextMeshProUGUI ammoText;

    [Header("Weapon Components")]
    [SerializeField] AudioSource aud;
    [SerializeField] Camera gameplayCamera;
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

    private bool canFire = true;
    private PlayerMovement movement;

    //melee
    private float meleeCooldownTimer;

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

    public void GetGunStats(WeaponStats gun, int startingAmmo = -1, int reserveAmmo = -1)// Optional Parameters to modify starting ammo
    {
        WeaponStats runtimeWeapon = Instantiate(gun);

        if (runtimeWeapon is FireArmStats firearm)
        {
            firearm.ammoCur = (startingAmmo >= 0) ? startingAmmo : firearm.ammoMax;
            firearm.ammoReserve = (reserveAmmo >= 0) ? reserveAmmo : firearm.maxAmmoReserve;
        }

        weaponList.Add(runtimeWeapon);
        EquipWeapon(weaponList.Count - 1);
    }

    public void HandleShooting()//Handles Shooting
    {
        if (!canFire) return;
        if (movement != null && movement.canSprint && Input.GetButton("Sprint"))
            return;

        shootCooldown -= Time.deltaTime;
        if (weaponList.Count == 0) return;
        FireArmStats currentGun = weaponList[currentWeaponIndex] as FireArmStats;
        if (currentGun == null || isReloading) return;

        bool fireInput = isAutomaticMode ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        if (fireInput && shootCooldown <= 0f)
        {
            if (currentGun.ammoCur > 0) // Check if mag is not empty, then fire
            {
                shootCooldown = isAutomaticMode ? currentGun.autoFireRate : currentGun.semiFireRate;
                Shoot();
                currentGun.ammoCur--;
                ammoText.SetText(currentGun.ammoCur + " / " + currentGun.ammoReserve);
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
    }

    void Shoot()//Handles damage/Ray cast/ and checks for current gun and gun stats
    {
        if (weaponList.Count == 0 || gameplayCamera == null) return;
        FireArmStats currentGun = weaponList[currentWeaponIndex] as FireArmStats;
        if (currentGun == null) return;

        if (currentGun.shootSound != null)
            aud.PlayOneShot(currentGun.shootSound);

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

            if (hit.collider.CompareTag("Enemy"))
            {
                iEnemyHealth enemyHealth = hit.collider.GetComponent<iEnemyHealth>();
                if (enemyHealth != null)
                {
                    GameManager.instance.SetCurrentEnemy(enemyHealth);
                }
            }
            else
            {
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

            IDamage dmg = hit.collider.GetComponent<IDamage>();
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
        }
        else
        {
            GameManager.instance.HideEnemyUI();
        }
    }

    public void HandleMeleeAttack()
    {
        meleeCooldownTimer -= Time.deltaTime;

        if (!Input.GetMouseButtonDown(1)) return; // Right-click for melee
        if (!HasGun()) return;

        var weapon = weaponList[currentWeaponIndex];

        if (weapon is not MeleeWeaponStats melee) return;
        if (meleeCooldownTimer > 0f) return;

        meleeCooldownTimer = melee.attackRate;

        if (animator != null)
        {
            animator.ResetTrigger("MeleeAttack");
            animator.SetTrigger("MeleeAttack");
        }

        if (melee.swingSound != null)
            aud.PlayOneShot(melee.swingSound);

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
        {
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
                dmg.takeDamage(melee.damage);

            if (melee.hitEffect != null)
                Instantiate(melee.hitEffect, hit.point, Quaternion.LookRotation(hit.normal));

            if (melee.hitSound != null)
                aud.PlayOneShot(melee.hitSound);
        }
    }


    IEnumerator ReloadRoutine(FireArmStats gun)//Handles reload and ammo limit reserve.
    {
        isReloading = true;
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetTrigger("Reload");

        if (gun.reloadSound != null) aud.PlayOneShot(gun.reloadSound);
        if (gun.reloadFreakingZombie != null) aud.PlayOneShot(gun.reloadFreakingZombie);
        yield return new WaitForSeconds(gun.reloadTime);

        int needed = gun.ammoMax - gun.ammoCur;
        if (gun.ammoReserve >= needed)
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
        ammoText.SetText(gun.ammoCur + " / " + gun.ammoReserve);
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
        if (weaponList.Count == 0) return;
        FireArmStats currentGun = weaponList[currentWeaponIndex] as FireArmStats;
        if (currentGun == null) return;

        if (currentGun.canSwitchFireMode)
        {
            isAutomaticMode = !isAutomaticMode;
            if (currentGun.fireModeSwitchSound != null)
                aud.PlayOneShot(currentGun.fireModeSwitchSound);
        }
    }

    public bool HasGun()//Checks if there is a current gun
    {
        return weaponList.Count > 0;
    }

    public FireArmStats CurrentGun
    {
        get
        {
            if (weaponList == null || weaponList.Count == 0)
                return null;
            return weaponList[currentWeaponIndex] as FireArmStats;
        }
    }

    public void AddAmmoToReserve(int ammoCount)
    {
        var gun = CurrentGun;
        if (gun == null) return;
        gun.ammoReserve += ammoCount;
        gun.ammoReserve = Mathf.Min(gun.ammoReserve, gun.maxAmmoReserve);
        StartCoroutine(AmmoFlash());
    }

    IEnumerator AmmoFlash()
    {
        GameManager.instance.flashAmmoPickUp.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.flashAmmoPickUp.SetActive(false);
        ammoText.SetText(CurrentGun.ammoCur + " / " + CurrentGun.ammoReserve);
    }

    public void UpdateAmmoUi()// Helper to update Ammo UI
    {
        if (HasGun())
        {
            FireArmStats gun = CurrentGun;
            ammoText.SetText(gun.ammoCur + " / " + gun.ammoReserve);
        }
    }

    public void EquipWeapon(int index)// Equips weapon and assings Gun icon
    {
        if (index < 0 || index >= weaponList.Count)
        {
            Debug.LogWarning("EquipWeapon: Invalid index");
            return;
        }

        currentWeaponIndex = index;

        WeaponStats weapon = weaponList[currentWeaponIndex];
        FireArmStats gun = weapon as FireArmStats;
        MeleeWeaponStats melee = weapon as MeleeWeaponStats;

        if (gun == null && melee == null) return;

        if (gun != null)
        {
            isAutomaticMode = gun.isAutomaticDefault;
        }

        if (currentWeaponInstance != null)
            Destroy(currentWeaponInstance);

        currentWeaponInstance = Instantiate(weapon.weaponModel, weaponHolder);
        currentWeaponInstance.transform.localPosition = Vector3.zero;
        //currentWeaponInstance.transform.localRotation = Quaternion.identity;

        currentHipPosition = currentWeaponInstance.transform.Find("HipPosition");
        Debug.Log("HipPosition local rotation: " + currentHipPosition.localRotation);
        Debug.Log("Hip world rotation" + currentHipPosition.rotation);
        currentAdsPosition = currentWeaponInstance.transform.Find("ADSPosition");
        barrelTip = currentWeaponInstance.transform.Find("BarrelTip");
        shellEjectionPoint = currentWeaponInstance.transform.Find("ShellEjection");


        if (gun != null)// new flag to check if the current equip weapon is a firearm, shows ammo, does not displays ammo for melee
        {
            ammoText.SetText(gun.ammoCur + " / " + gun.ammoReserve);
        }
        else
        {
            ammoText.SetText("∞");
        }

        var player = Object.FindFirstObjectByType<PlayerController>();
        bool isOneHanded = gun?.isOneHanded ?? melee?.isOneHanded ?? true;
        player.StartCoroutine(player.PlayPickupAnimation(isOneHanded));

        for (int i = 0; i < weaponIcons.Count; i++)
        {
            bool hasWeapon = i < weaponList.Count && weaponList[i]?.weaponIcon != null;

            if (weaponIcons[i] != null)
            {
                weaponIcons[i].enabled = hasWeapon;

                if (hasWeapon)
                {
                    weaponIcons[i].sprite = weaponList[i].weaponIcon;
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

    public void ScrollWeapon(int direction)
    {
        if (weaponList.Count == 0) return;

        int newIndex = (currentWeaponIndex + direction + weaponList.Count) % weaponList.Count;

        if (newIndex == currentWeaponIndex)
            return; // Don't re-equip the same weapon

        EquipWeapon(newIndex);
    }

    public void TryEquipWeapon(int index)
    {
        if (index < 0 || index >= weaponList.Count) return;
        if (index == currentWeaponIndex) return;

        EquipWeapon(index);
    }

    public void SetCanFire(bool value)
    {
        canFire = value;
    }
}
