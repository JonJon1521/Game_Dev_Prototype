using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage, IPickup
{
    // --- Added Reference ---
    private StatsManager stats;

    [Header("Spellcasting")]
    [SerializeField] private List<GameObject> spellLoadout = new List<GameObject>();
    [SerializeField] private List<Spellstats> spellStats = new List<Spellstats>();
    [SerializeField] ManaSystem manaSystem;
    [SerializeField] Spellslot spellslot;
    [SerializeField] private Transform castPoint;

    [Header("Components")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("Stats (Managed by StatsManager)")]
    [SerializeField] int currentHP;
    [SerializeField] int currentMana;
    [SerializeField] int sprintMod = 2;
    [SerializeField] int jumpSpeed = 15;
    [SerializeField] int jumptimesMax = 2;
    [SerializeField] int gravity = 30;

    [Header("Dodge Settings")]
    [SerializeField] float dodgeDistance = 3f;
    [SerializeField] float dodgeSpeed = 8f;

    [Header("Guns")]
    [SerializeField] List<gunStats> gunList = new List<gunStats>();
    [SerializeField] GameObject gunModel;

    [Header("Audio")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audJump;
    [SerializeField] float audJumpVol;
    [SerializeField] AudioClip[] audStep;
    [SerializeField] float audStepVol;
    [SerializeField] AudioClip[] audHurt;
    [SerializeField] float audHurtVol;

    // Internal tracking
    int jumpCount;
    int gunListPos;
    float shootTimer;
    bool isPlayingStep;
    bool isSprinting;
    Vector3 moveDir;
    Vector3 playerVel;
    bool isDodging = false;

    public Transform cameratransform;
    public int CurrentGunPos => gunListPos;
    private GameObject[] activeSpells;

    List<int> gunAmmoCur = new List<int>();
    List<int> gunTotalAmmo = new List<int>();
    List<int> gunAmmoMaxOriginal = new List<int>();
    List<int> gunAmmoCurOriginal = new List<int>();
    List<int> gunAmmoMaxSession = new List<int>();

    void Start()
    {
        // 1. Grab the StatsManager
        stats = GetComponent<StatsManager>();

        // 2. Initialize the player
        spawnPlayer();

        // 3. Setup Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (gunList.Count > 0)
            gunListPos = 0;

        if (gamemanager.instance != null)
            gamemanager.instance.UpdateSpellUI(spellLoadout);
    }

    void Update()
    {
        if (gamemanager.instance != null && !gamemanager.instance.isPaused)
        {
            movement();
            sprint();
            HandleDodgeInput();

            if (Input.GetKeyDown(KeyCode.R)) reload();
            if (Input.GetKeyDown(KeyCode.C)) CastSpell(0);
            if (Input.GetKeyDown(KeyCode.V)) CastSpell(1);
        }
    }

    public void spawnPlayer()
    {
        controller.transform.position = gamemanager.instance.playerSpawnPos.transform.position;
        Physics.SyncTransforms();

        // Pull fresh max values from StatsManager
        currentHP = (int)stats.maxHealth;
        currentMana = (int)stats.maxMana;

        updatePlayerUI();

        // Reset gun ammo lists if they have been initialized
        for (int i = 0; i < gunList.Count; i++)
        {
            if (gunAmmoCur.Count > i) gunAmmoCur[i] = gunAmmoCurOriginal[i];
            if (gunAmmoMaxSession.Count > i) gunAmmoMaxSession[i] = gunAmmoMaxOriginal[i];
        }

        if (gunList.Count > 0)
            gamemanager.instance.updateAmmoUI(gunAmmoCur[gunListPos], gunAmmoMaxSession[gunListPos]);
    }

    void movement()
    {
        shootTimer += Time.deltaTime;

        if (controller.isGrounded)
        {
            playerVel.y = 0f;
            jumpCount = 0;
        }

        // Camera-based direction
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        moveDir = (cameraForward * Input.GetAxis("Vertical")) + (cameraRight * Input.GetAxis("Horizontal"));

        if (moveDir.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
        }

        // CALCULATE SPEED: Base Speed from StatsManager * Sprint Multiplier
        float moveSpeed = isSprinting ? stats.moveSpeed * sprintMod : stats.moveSpeed;
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;

        if (Input.GetButtonDown("Fire1") && gunList.Count > 0 && gunAmmoCur[gunListPos] > 0 && shootTimer >= gunList[gunListPos].shootRate)
        {
            shoot();
        }

        selectGun();

        if (aud != null && audStep.Length > 0 && moveDir.normalized.magnitude > 0.3f && !isPlayingStep)
        {
            StartCoroutine(playStep());
        }
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumptimesMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
            if (audJump.Length > 0)
                aud.PlayOneShot(audJump[UnityEngine.Random.Range(0, audJump.Length)], audJumpVol);
        }
    }

    void sprint()
    {
        // Simple toggle for movement calculation
        if (Input.GetButtonDown("Sprint")) isSprinting = true;
        else if (Input.GetButtonUp("Sprint")) isSprinting = false;
    }

    public void takeDamage(int damage)
    {
        currentHP -= damage;
        updatePlayerUI();
        
        if (audHurt.Length > 0)
            aud.PlayOneShot(audHurt[UnityEngine.Random.Range(0, audHurt.Length)], audHurtVol);
        
        StartCoroutine(flashDamage());

        if (currentHP <= 0)
        {
            gamemanager.instance.youLose();
        }
    }

    public void updatePlayerUI()
    {
        // Calculate fill based on Max Stats in the StatsManager
        gamemanager.instance.playerHPBar.fillAmount = (float)currentHP / stats.maxHealth;
        gamemanager.instance.playerManaBar.fillAmount = (float)currentMana / stats.maxMana;
    }

    public void heal(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, (int)stats.maxHealth);
        updatePlayerUI();
    }

    public void restoreMana(int amount)
    {
        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, (int)stats.maxMana);
        updatePlayerUI();
    }
    public void EquipSpell(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= spellLoadout.Count) return;

        GameObject spellPrefab = spellLoadout[slotIndex];
        if (spellPrefab == null) return;

        if (activeSpells == null || activeSpells.Length != spellLoadout.Count)
            activeSpells = new GameObject[spellLoadout.Count];

        if (activeSpells[slotIndex] != null)
            Destroy(activeSpells[slotIndex]);

        activeSpells[slotIndex] = Instantiate(
            spellPrefab,
            transform.position,
            Quaternion.identity,
            transform
        );

        gamemanager.instance.UpdateSpellUI(spellLoadout);
    }

    void CastSpell(int index)
    {
        if (spellslot == null || index >= spellLoadout.Count) return;

        Spellstats sStats = spellslot.GetSpell(index);
        if (sStats == null) return;

        // Check if we have enough mana
        if (currentMana >= sStats.manaCost)
        {
            currentMana -= (int)sStats.manaCost;
            updatePlayerUI();
            Instantiate(spellLoadout[index], castPoint.position, castPoint.rotation);
        }
        else
        {
            Debug.Log("Not enough mana!");
        }
    }

    public void AddAmmo(int amount)
    {
        if (gunList.Count == 0) return;

        gunAmmoMaxSession[gunListPos] += amount; // add to the reserved (pockets) , not the current ammo

        // Clamp to session max
        if (gunAmmoCur[gunListPos] > gunAmmoMaxSession[gunListPos])
            gunAmmoCur[gunListPos] = gunAmmoMaxSession[gunListPos];

        gamemanager.instance.updateAmmoUI(gunAmmoCur[gunListPos], gunAmmoMaxSession[gunListPos]);
    }


    void shoot()
    {
        if (gunList.Count == 0) return;
        gunStats gun = gunList[gunListPos];
        if (gunAmmoCur[gunListPos] <= 0) return;

        shootTimer = 0;
        gunAmmoCur[gunListPos]--;
        gamemanager.instance.updateAmmoUI(gunAmmoCur[gunListPos], gunAmmoMaxSession[gunListPos]);
        aud.PlayOneShot(gun.shootSound[UnityEngine.Random.Range(0, gun.shootSound.Length)], gun.shootSoundVol);

        Vector3 rayOrigin = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
        int layerMask = ~LayerMask.GetMask("Gun Camera", "Player");

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, Camera.main.transform.forward, out hit, gun.shootDist, layerMask))
        {
            if (gun.hitEffect != null)
            {
                ParticleSystem effect = Instantiate(gun.hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                effect.Play();
                Destroy(effect.gameObject, 2f);
            }

            IDamage dmg = hit.collider.GetComponentInParent<IDamage>();
            if (dmg != null) dmg.takeDamage(gun.shootDamage);
        }
    }

    void HandleDodgeInput()
    {
        if (isDodging) return;
        Camera cam = Camera.main;
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(DodgeRoutine(transform.right));
            if (cam != null && cam.GetComponent<cameraController>() != null)
                cam.GetComponent<cameraController>().DodgeTilt(-40f, 0.2f);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(DodgeRoutine(-transform.right));
            if (cam != null && cam.GetComponent<cameraController>() != null)
                cam.GetComponent<cameraController>().DodgeTilt(40f, 0.2f);
        }
    }

    private IEnumerator DodgeRoutine(Vector3 dir)
    {
        isDodging = true;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + dir * dodgeDistance;
        float t = 0f;
        while (t < 1f)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            t += Time.deltaTime * dodgeSpeed;
            yield return null;
        }
        yield return new WaitForSeconds(0.05f);
        isDodging = false;
    }

    IEnumerator playStep()
    {
        isPlayingStep = true;
        if (aud != null && audStep.Length > 0)
            aud.PlayOneShot(audStep[UnityEngine.Random.Range(0, audStep.Length)], audStepVol);

        yield return new WaitForSeconds(isSprinting ? 0.3f : 0.5f);
        isPlayingStep = false;
    }

    IEnumerator flashDamage()
    {
        gamemanager.instance.damagePlayerFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gamemanager.instance.damagePlayerFlash.SetActive(false);
    }

    public void reload()
    {
        if (gunList.Count == 0 || gunListPos >= gunList.Count) return;
        int missingAmmo = gunAmmoCurOriginal[gunListPos] - gunAmmoCur[gunListPos];
        if (missingAmmo > gunAmmoMaxSession[gunListPos]) missingAmmo = gunAmmoMaxSession[gunListPos];

        if (missingAmmo > 0)
        {
            gunAmmoCur[gunListPos] += missingAmmo;
            gunAmmoMaxSession[gunListPos] -= missingAmmo;
            gamemanager.instance.updateAmmoUI(gunAmmoCur[gunListPos], gunAmmoMaxSession[gunListPos]);
            if (gunList[gunListPos].reloadSound != null)
                aud.PlayOneShot(gunList[gunListPos].reloadSound, gunList[gunListPos].reloadSoundVol);
        }
    }

    public void getGunStats(gunStats gun)
    {
        gunList.Add(gun);
        gunAmmoCur.Add(gun.ammoCur);
        gunTotalAmmo.Add(gun.totalAmmo);
        gunAmmoCurOriginal.Add(gun.ammoCur);
        gunAmmoMaxOriginal.Add(gun.ammoMax);
        gunAmmoMaxSession.Add(gun.ammoMax);
        gunListPos = gunList.Count - 1;
        changeGun();
    }

    void changeGun()
    {
        gunModel.GetComponent<MeshFilter>().mesh = gunList[gunListPos].gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunList[gunListPos].gunModel.GetComponent<MeshRenderer>().sharedMaterial;
        gamemanager.instance.updateAmmoUI(gunAmmoCur[gunListPos], gunAmmoMaxSession[gunListPos]);
    }

    void selectGun()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPos < gunList.Count - 1)
        {
            gunListPos++;
            changeGun();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && gunListPos > 0)
        {
            gunListPos--;
            changeGun();
        }
    }
    public void removeSlowSpeed(int amount)
    {
        sprintMod *= amount;
    }
    public void applySlowSpeed(int amount)
    {
        sprintMod /= amount;
    }

}