using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using System.Linq;
using UnityEngine.UI;
using NUnit.Framework;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Starting Weapon")]
    [SerializeField] private MeleeWeaponStats startingKnife;


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

    [Header("Fire mode Icons")]
    [SerializeField] private Image fireModeIcon;
    [SerializeField] private Sprite semiFireModeIcon;
    [SerializeField] private Sprite fullFireModeIcon;

    [Header("Stamina Settings")]
    [SerializeField] private float heavyStaminaThreshold = 5f;
    [SerializeField] private float lightStaminaThreshold = 1f;
    [SerializeField] private float lightAttackStaminaCost = 1f;
    [SerializeField] private float heavyAttackStaminaCost = 5f;


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

        if (startingKnife != null)
            GetGunStats(startingKnife, autoEquip: true);

    }

    public void GetGunStats(WeaponStats gun, int startingAmmo = -1, int reserveAmmo = -1, bool autoEquip = true)// Optional Parameters to modify starting ammo and auto equip
    {
        if (HasGun(gun.weaponNameId))
        {
            if (gun is FireArmStats firearm)
            {
                AddAmmoToReserve(firearm.ammoType, reserveAmmo >= 0 ? reserveAmmo : Random.Range(5, 30));
            }
            return;
        }

        WeaponStats runtimeWeapon = Instantiate(gun);

        if (runtimeWeapon is FireArmStats newFirearm)
        {
            newFirearm.ammoCur = (startingAmmo >= 0) ? startingAmmo : newFirearm.ammoMax;
            newFirearm.ammoReserve = (reserveAmmo >= 0) ? reserveAmmo : newFirearm.maxAmmoReserve;
        }

        weaponList.Add(runtimeWeapon);

        if (autoEquip)
            EquipWeapon(weaponList.Count - 1);
    }


    public void HandleShooting()//Handles Shooting
    {
        if (!canFire) return;
        if (movement != null && movement.IsActuallySprinting())
            return;

        shootCooldown -= Time.deltaTime;
        if (weaponList.Count == 0) return;
        FireArmStats currentGun = weaponList[currentWeaponIndex] as FireArmStats;
        if (currentGun == null || isReloading) return;

        bool fireInput = isAutomaticMode ?
            PlayerController.inputActions.Input.Fire.ReadValue<float>() > 0.1f :
            PlayerController.inputActions.Input.Fire.triggered;

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

        if (PlayerController.inputActions.Input.Fire.ReadValue<float>() == 0f)
            playedEmptySound = false;

        bool reloadInput = PlayerController.inputActions.Input.Reload.IsPressed();

        if (reloadInput && currentGun.ammoCur < currentGun.ammoMax && currentGun.ammoReserve > 0 && !isReloading)
            reloadCoroutine = StartCoroutine(ReloadRoutine(currentGun));// Starts Reload
    }

    void Shoot()//Handles damage/Ray cast/ and checks for current gun and gun stats
    {
        if (weaponList.Count == 0 || gameplayCamera == null || barrelTip == null) return;

        FireArmStats currentGun = weaponList[currentWeaponIndex] as FireArmStats;
        if (currentGun == null) return;

        if (currentGun.shootSound != null)
            aud.PlayOneShot(currentGun.shootSound);

        // Ray Setup
        Ray ray;
        if (isAiming)
            ray = gameplayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        else
        {
            float spreadAngle = 2f;
            Vector3 spreadDir = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0
            ) * barrelTip.forward;
            ray = new Ray(barrelTip.position, spreadDir);
        }

        Debug.DrawRay(ray.origin, ray.direction * currentGun.shootRange, Color.red, 1f);

        // Muzzle flash
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, barrelTip.position, barrelTip.rotation, barrelTip);
            Destroy(flash, 0.1f);
        }

        // Perform raycast
        RaycastHit[] hits = Physics.RaycastAll(ray, currentGun.shootRange, ~ignoreLayer, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Find valid hit
        RaycastHit? validHit = null;
        foreach (var h in hits)
        {
            if (h.collider != null && !h.collider.CompareTag("Lights") && !h.collider.CompareTag("Player"))
            {
                validHit = h;
                break;
            }
        }

        //Always eject shell
        if (shellCasingPrefab != null && shellEjectionPoint != null)
        {
            GameObject shell = Instantiate(shellCasingPrefab, shellEjectionPoint.position, shellEjectionPoint.rotation);
            Rigidbody rb = shell.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 ejectDirection = shellEjectionPoint.right +
                    (shellEjectionPoint.up * Random.Range(-0.2f, 0.2f)) +
                    (shellEjectionPoint.forward * Random.Range(-0.1f, 0.1f));
                rb.AddForce(ejectDirection.normalized * shellEjectForce, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * shellEjectForce, ForceMode.Impulse);
            }
            Destroy(shell, 3f);
        }

        if (validHit.HasValue)
        {
            RaycastHit hit = validHit.Value;

            // enemy UI
            if (hit.collider.CompareTag("Enemy"))
            {
                iEnemyHealth enemyHealth = hit.collider.GetComponent<iEnemyHealth>();
                if (enemyHealth != null)
                    GameManager.instance.SetCurrentEnemy(enemyHealth);
            }
            else
            {
                GameManager.instance.HideEnemyUI();
            }

            // hit VFX
            if (currentGun.hitEffect != null)
                Instantiate(currentGun.hitEffect, hit.point, Quaternion.LookRotation(hit.normal));

            if (!hit.collider.CompareTag("Enemy") && currentGun.bulletHolePrefab != null)
            {
                GameObject bulletHole = Instantiate(currentGun.bulletHolePrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
                bulletHole.transform.SetParent(hit.transform);
                Destroy(bulletHole, 10f);
            }
            else if (hit.collider.CompareTag("Enemy") && currentGun.zombieBloodHit != null)
            {
                GameObject bloodEffect = Instantiate(currentGun.zombieBloodHit, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
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

            currentGunOffset.y -= weaponRecoilKick;
            currentLeftHandOffset.z -= handRecoilKick;
            currentRightHandOffset.z -= handRecoilKick;
        }
        else
        {
            GameManager.instance.HideEnemyUI();
            if (tracerPrefab != null)
            {
                GameObject tracer = Instantiate(tracerPrefab, barrelTip.position, Quaternion.LookRotation(ray.direction));
                Rigidbody rb = tracer.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddForce(ray.direction * 1000f, ForceMode.Impulse);
                Destroy(tracer, 2f);
            }
        }
    }


    public void HandleMeleeAttack()// Handles melee attacks light and heavy
    {
        if (!canFire || !HasGun() || movement == null) return;

        var weapon = weaponList[currentWeaponIndex];
        if (weapon is not MeleeWeaponStats melee) return;

        meleeCooldownTimer -= Time.deltaTime;

        bool heavyAttackHeld = PlayerController.inputActions.Input.ADS.IsPressed();
        bool normalAttackPressed = PlayerController.inputActions.Input.Fire.IsPressed();

        if (heavyAttackHeld && movement.currentStamina >= heavyStaminaThreshold && meleeCooldownTimer <= 0f)
        {
            PerformHeavyAttack(melee);
        }
        else if (normalAttackPressed && movement.currentStamina >= lightStaminaThreshold && meleeCooldownTimer <= 0f)
        {
            PerformNormalAttack(melee);
        }
    }

    public void PerformHeavyAttack(MeleeWeaponStats melee)
    {
        // Drain stamina for the heavy attack
        movement.currentStamina -= heavyAttackStaminaCost;
        movement.currentStamina = Mathf.Max(movement.currentStamina, 0f); // Prevent negative stamina

        // Reset cooldown for heavy attack
        meleeCooldownTimer = melee.attackRate;

        if (animator != null)
        {
            animator.ResetTrigger("MeleeAttack");
            animator.SetTrigger("HeavyAttack");
        }

        // Perform the attack (same as normal attack but with more damage)
        Collider hitbox = currentWeaponInstance.GetComponentInChildren<Collider>();
        StartCoroutine(EnableKnifeHitBox(hitbox, 0.5f)); // Increased duration for heavy attack hitbox

        if (melee.swingSound != null)
            aud.PlayOneShot(melee.swingSound);

        // Perform hit detection
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(ray.origin, ray.direction * 2f, Color.red, 1f);

        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, 2f);

        if (hitSomething)
        {

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            iEnemyHealth enemyHealth = hit.collider.GetComponent<iEnemyHealth>();

            if (dmg != null)
                dmg.takeDamage(melee.damage * 2); // More damage for heavy attack

            if (hit.collider.CompareTag("Enemy"))
            {     
                    if (melee.zombieHit != null)
                    aud.PlayOneShot(melee.zombieHit);

                    if (enemyHealth != null)
                    {
                        if (enemyHealth.CurrentHealth <= 0)
                        {
                            GameManager.instance.HideEnemyUI();
                        }
                        else
                        {
                            GameManager.instance.SetCurrentEnemy(enemyHealth);
                        }
                    }
             }
             else
             {
                GameManager.instance.HideEnemyUI();
             }
        }
        else
        {
            GameManager.instance.HideEnemyUI();
        }
    }

    public void PerformNormalAttack(MeleeWeaponStats melee)
    {
        meleeCooldownTimer = melee.attackRate;
        movement.currentStamina -= lightAttackStaminaCost;
        movement.currentStamina = Mathf.Max(movement.currentStamina, 0f);
        // Reset trigger and trigger normal attack animation
        if (animator != null)
        {
            animator.ResetTrigger("HeavyAttack");
            animator.SetTrigger("MeleeAttack");
        }

        // Perform the attack
        Collider hitbox = currentWeaponInstance.GetComponentInChildren<Collider>();
        StartCoroutine(EnableKnifeHitBox(hitbox, 0.3f));

        if (melee.swingSound != null)
            aud.PlayOneShot(melee.swingSound);

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(ray.origin, ray.direction * 2f, Color.red, 1f);

        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, 2f);

        if (hitSomething)
        {

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            iEnemyHealth enemyHealth = hit.collider.GetComponent<iEnemyHealth>();

            if (dmg != null)
                dmg.takeDamage(melee.damage); // More damage for heavy attack

            if (hit.collider.CompareTag("Enemy"))
            {
                if (melee.zombieHit != null)
                    aud.PlayOneShot(melee.zombieHit);

                if (enemyHealth != null)
                {
                    if (enemyHealth.CurrentHealth <= 0)
                    {
                        GameManager.instance.HideEnemyUI();
                    }
                    else
                    {
                        GameManager.instance.SetCurrentEnemy(enemyHealth);
                    }
                }
            }
            else
            {
                GameManager.instance.HideEnemyUI();
            }
        }
        else
        {
            GameManager.instance.HideEnemyUI();
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
        // Block ADS if current weapon is melee
        if (weaponList.Count == 0 || currentWeaponIndex >= weaponList.Count)
            return;

        if (weaponList[currentWeaponIndex] is MeleeWeaponStats)
        {
            isAiming = false;
            if (animator != null)
                animator.SetBool("IsAiming", false);
            return;
        }

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
        if (currentGun == null || !currentGun.canSwitchFireMode) return;

        isAutomaticMode = !isAutomaticMode;

        if (currentGun.fireModeSwitchSound != null)
            aud.PlayOneShot(currentGun.fireModeSwitchSound);

        UpdateFireModeUI();
    }

    public bool HasGun() // This checks if player has any weapon
    {
        return weaponList.Count > 0;
    }

    public bool HasGun(string weaponId)//This checks if player has a weapon with specific id 
    {
        return weaponList.Any(w => w.weaponNameId == weaponId);
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

    public bool AddAmmoToReserve(AmmoTypes ammoType, int ammoCount)
    {
        // Find all guns that use this ammo type
        var matchingGuns = weaponList.OfType<FireArmStats>().Where(g => g.ammoType == ammoType).ToList();
        if (matchingGuns.Count == 0) return false;

        // Prefer the currently equipped gun if it matches
        FireArmStats targetGun = matchingGuns.FirstOrDefault(g => g == CurrentGun && g.ammoType == ammoType)
                                 ?? matchingGuns.OrderBy(g => g.ammoReserve).First();

        if (targetGun.ammoReserve >= targetGun.maxAmmoReserve)
        {
            Debug.Log($"[PlayerWeaponManager] {ammoType} ammo already full.");
            return false;
        }

        targetGun.ammoReserve += ammoCount;
        targetGun.ammoReserve = Mathf.Min(targetGun.ammoReserve, targetGun.maxAmmoReserve);

        StartCoroutine(AmmoFlash());
        return true;
    }
    public bool HasWeaponWithAmmoType(AmmoTypes type)
    {
        return weaponList.OfType<FireArmStats>().Any(g => g.ammoType == type);
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
        UpdateFireModeUI();
        if (currentWeaponInstance != null)
            Destroy(currentWeaponInstance);

        currentWeaponInstance = Instantiate(weapon.weaponModel, weaponHolder);
        currentWeaponInstance.transform.localPosition = weapon.weaponModel.transform.localPosition;
        currentWeaponInstance.transform.localRotation = weapon.weaponModel.transform.localRotation;

        if (melee != null)// Assings melee damage from melee stats
        {
            MeleeWeaponHitbox hitbox = currentWeaponInstance.GetComponentInChildren<MeleeWeaponHitbox>();
            if (hitbox != null)
                hitbox.SetDamage(melee.damage);
        }


        currentHipPosition = currentWeaponInstance.transform.Find("HipPosition");
        Debug.Log("HipPosition local rotation: " + currentHipPosition.localRotation);
        Debug.Log("Hip world rotation" + currentHipPosition.rotation);
        currentAdsPosition = currentWeaponInstance.transform.Find("ADSPosition");
        barrelTip = currentWeaponInstance.transform.Find("BarrelTip");
        shellEjectionPoint = currentWeaponInstance.transform.Find("ShellEjection");


        if (gun != null)// new flag to check if the current equip weapon is a firearm, shows ammo, does not displays ammo for melee
        {
            currentAdsPosition = currentWeaponInstance.transform.Find("ADSPosition");
            barrelTip = currentWeaponInstance.transform.Find("BarrelTip");
            shellEjectionPoint = currentWeaponInstance.transform.Find("ShellEjection");
            ammoText.SetText(gun.ammoCur + " / " + gun.ammoReserve);
        }
        else
        {
            // Melee weapon — disable ADS
            currentAdsPosition = null;
            barrelTip = null;
            shellEjectionPoint = null;
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

    IEnumerator EnableKnifeHitBox(Collider hitbox, float duration)
    {
        hitbox.enabled = true;

        // Force overlap check
        Vector3 center = hitbox.bounds.center;
        Vector3 halfExtents = hitbox.bounds.extents;
        Collider[] hits = Physics.OverlapBox(center, halfExtents, hitbox.transform.rotation, ~ignoreLayer);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                IDamage dmg = hit.GetComponent<IDamage>();
                if (dmg != null)
                {
                    dmg.takeDamage(weaponList[currentWeaponIndex] is MeleeWeaponStats melee ? melee.damage : 10);
                }

                iEnemyHealth enemyHealth = hit.GetComponent<iEnemyHealth>();
                if (enemyHealth != null)
                {
                    GameManager.instance.SetCurrentEnemy(enemyHealth);
                }

                // Play zombie hit sound
                if (weaponList[currentWeaponIndex] is MeleeWeaponStats meleeStats && meleeStats.zombieHit != null)
                {
                    aud.PlayOneShot(meleeStats.zombieHit);
                }
            }
        }

        yield return new WaitForSeconds(duration);
        hitbox.enabled = false;
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

    private void UpdateFireModeUI()// Changes firemode ICon
    {
        if (fireModeIcon == null)
            return;

        FireArmStats gun = weaponList[currentWeaponIndex] as FireArmStats;

        if (gun == null)
        {
            fireModeIcon.enabled = false; // Hide icon for melee or null
        }
        else
        {
            fireModeIcon.enabled = true;
            fireModeIcon.sprite = isAutomaticMode ? fullFireModeIcon : semiFireModeIcon;
        }
    }

   

}

