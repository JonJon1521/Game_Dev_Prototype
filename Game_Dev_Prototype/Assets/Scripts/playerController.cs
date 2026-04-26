using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;

public class playerController : MonoBehaviour, IDamage, IPickup
{

    [Header("Spellcasting")]
    [SerializeField] private List<GameObject> spellLoadout = new List<GameObject>();
    [SerializeField] private List<Spellstats> spellStats = new List<Spellstats>();
    [SerializeField] ManaSystem manaSystem;
    [SerializeField] Spellslot spellslot;

    [SerializeField] private Transform castPoint;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("Compontents")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    //~~~~~~~~~~~~~~~~~~~~Stats (ints)~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("Stats")]
    [Range(1, 100)][SerializeField] int HP;
    [Range(1, 100)][SerializeField] int Mana;
    [Range(1, 10)][SerializeField] int speed;
    [Range(2, 6)][SerializeField] int sprintMod;
    [Range(5, 25)][SerializeField] int jumpSpeed;
    [Range(1, 4)][SerializeField] int jumptimesMax;
    [Range(15, 50)][SerializeField] int gravity;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("Dodge Settings")]
    [SerializeField] float dodgeDistance = 3f;
    [SerializeField] float dodgeSpeed = 8f;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("Guns")]
    [SerializeField] List<gunStats> gunList = new List<gunStats>();
    [SerializeField] GameObject gunModel;

    //~~~~~~~~~~~~~~~~~~~~Audio~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("Audio")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audJump;
    [SerializeField] float audJumpVol;
    [SerializeField] AudioClip[] audStep;
    [SerializeField] float audStepVol;
    [SerializeField] AudioClip[] audHurt;
    [SerializeField] float audHurtVol;

    //~~~~~~~~~~~~~~~~~~~~~INTS~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    int jumpCount;
    int HPOriginal;
    int ManaOriginal;
    int gunListPos;

    //~~~~~~~~~~~~~~~~~~~~~FLOATS~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    float shootTimer;

    //~~~~~~~~~~~~~~~~~~~~~BOOLS~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    bool isPlayingStep;
    bool isSprinting;

    //~~~~~~~~~~~~~~~~~~~~~VECTORS~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    Vector3 moveDir;
    Vector3 playerVel;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    bool isDodging = false;

    public Transform cameratransform;
    public int CurrentGunPos => gunListPos;

    private GameObject[] activeSpells;



    List<int> gunAmmoCur = new List<int>();
    List<int> gunTotalAmmo = new List<int>();
    List<int> gunAmmoMaxOriginal = new List<int>();
    List<int> gunAmmoCurOriginal = new List<int>();
    List<int> gunAmmoMaxSession = new List<int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOriginal = HP;

        ManaOriginal = Mana;

        spawnPlayer();

        // adding these to hide the mouse ~~~~~

        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        if (gunList.Count > 0)
            gunListPos = 0;

        gamemanager.instance.UpdateSpellUI(spellLoadout);
    }

    // Update is called once per frame
    void Update()
    {
        if (gamemanager.instance != null && !gamemanager.instance.isPaused)
        {
            movement();
            sprint();
            HandleDodgeInput();

            if (Input.GetKeyDown(KeyCode.R))
            {
                reload();
            }
            if (Input.GetKeyDown(KeyCode.C))
                CastSpell(0);

            if (Input.GetKeyDown(KeyCode.V))
                CastSpell(1);

        }
    }
    IEnumerator playStep()
    {
        isPlayingStep = true;

        if (aud != null && audStep != null && audStep.Length > 0)
        {
            aud.PlayOneShot(audStep[UnityEngine.Random.Range(0, audJump.Length)], audStepVol);
        }

        if (isSprinting)
        {
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        isPlayingStep = false;


    }

    public void spawnPlayer()
    {
        controller.transform.position = gamemanager.instance.playerSpawnPos.transform.position;
        Physics.SyncTransforms();

        HP = HPOriginal;

        Mana = ManaOriginal;

        updatePlayerUI();

        // Reset all ammo and session max
        for (int i = 0; i < gunList.Count; i++)
        {
            gunAmmoCur[i] = gunAmmoCurOriginal[i];
            gunAmmoMaxSession[i] = gunAmmoMaxOriginal[i];
        }

        // Update UI for current gun
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

        // for cut scene and camera usint the cinamatic cameras 

        // get teh camera directions

        Vector3 cameraForward = Camera.main.transform.forward;

        Vector3 cameraRight = Camera.main.transform.right;

        //remove the y so the player dosent walk into the ground 

        cameraForward.y = 0;

        cameraRight.y = 0;

        cameraForward = cameraForward.normalized;

        cameraRight = cameraRight.normalized;

        // calculat moveDir based on camera instead of  transform 

        moveDir = (cameraForward * Input.GetAxis("Vertical")) + (cameraRight * Input.GetAxis("Horizontal"));

        if (moveDir.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
        }

        controller.Move(moveDir * speed * Time.deltaTime);

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
            aud.PlayOneShot(audJump[UnityEngine.Random.Range(0, audJump.Length)], audJumpVol);
        }
    }


    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
            isSprinting = true;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
            isSprinting = false;
        }
    }


    void shoot()
    {
        if (gunList.Count == 0) return;

        gunStats gun = gunList[gunListPos];

        // If still empty after reload, cannot shoot
        if (gunAmmoCur[gunListPos] <= 0) return;

        shootTimer = 0;

        // Fire the shot
        gunAmmoCur[gunListPos]--;
        gamemanager.instance.updateAmmoUI(gunAmmoCur[gunListPos], gunAmmoMaxSession[gunListPos]);

        // Play shooting sound
        aud.PlayOneShot(gun.shootSound[UnityEngine.Random.Range(0, gun.shootSound.Length)], gun.shootSoundVol);

        // Raycast start slightly in front of camera to avoid hitting gun
        Vector3 rayOrigin = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;

        // Ignore Gun Camera layer (includes gun model) and Player
        int layerMask = ~LayerMask.GetMask("Gun Camera", "Player");

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, Camera.main.transform.forward, out hit, gun.shootDist, layerMask))
        {
            // Spawn hit effect (appears at hit point)
            if (gun.hitEffect != null)
            {
                ParticleSystem effect = Instantiate(gun.hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                effect.Play();
                Destroy(effect.gameObject, 2f);
            }

            // Apply damage
            IDamage dmg = hit.collider.GetComponentInParent<IDamage>();
            if (dmg != null)
                dmg.takeDamage(gun.shootDamage);
        }
    }

    void HandleDodgeInput()
    {
        if (isDodging) return;

        Camera cam = Camera.main;
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(DodgeRoutine(transform.right)); // move player
            if (cam != null)
            {
                cam.GetComponent<cameraController>().DodgeTilt(-40f, 0.2f); // tilt camera left
                cam.GetComponent<cameraController>().shack(0.2f, 0.1f);      // optional shake
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(DodgeRoutine(-transform.right)); // move player
            if (cam != null)
            {
                cam.GetComponent<cameraController>().DodgeTilt(40f, 0.2f);  // tilt camera right
                cam.GetComponent<cameraController>().shack(0.2f, 0.1f);    // optional shake
            }
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

    public void takeDamage(int damage)
    {
        HP -= damage;
        updatePlayerUI();
        aud.PlayOneShot(audHurt[UnityEngine.Random.Range(0, audHurt.Length)], audHurtVol);
        StartCoroutine(flashDamage());
        if (HP <= 0)
        {
            // Fully qualify if needed
            global::gamemanager.instance.youLose();
        }
    }
    IEnumerator flashDamage()
    {
        gamemanager.instance.damagePlayerFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gamemanager.instance.damagePlayerFlash.SetActive(false);
    }



    public void updatePlayerUI()
    {
        gamemanager.instance.playerHPBar.fillAmount = (float)HP / HPOriginal;
        gamemanager.instance.playerManaBar.fillAmount = (float)Mana / ManaOriginal;
    }


    public void getGunStats(gunStats gun)
    {
        gunList.Add(gun);
        gunAmmoCur.Add(gun.ammoCur);
        gunTotalAmmo.Add(gun.totalAmmo);

        // Store originals
        gunAmmoCurOriginal.Add(gun.ammoCur);
        gunAmmoMaxOriginal.Add(gun.ammoMax);

        // Session max for depletion
        gunAmmoMaxSession.Add(gun.ammoMax);

        gunListPos = gunList.Count - 1;
        changeGun();
        gamemanager.instance.updateAmmoUI(gunAmmoCur[gunListPos], gunAmmoMaxSession[gunListPos]);
    }

    void changeGun()
    {
        gunModel.GetComponent<MeshFilter>().mesh = gunList[gunListPos].gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunList[gunListPos].gunModel.GetComponent<MeshRenderer>().sharedMaterial;
        gamemanager.instance.updateAmmoUI(gunAmmoCur[gunListPos], gunList[gunListPos].ammoMax);
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
        speed *= amount;
    }
    public void applySlowSpeed(int amount)
    {
        speed /= amount;
    }
    public void heal(int amount)
    {
        HP += amount;
        HP = Mathf.Clamp(HP, 0, HPOriginal);
        updatePlayerUI();

    }
    public void restoreMana(int amount)
    {
        Mana += amount;

        Mana = Mathf.Clamp(Mana, 0, ManaOriginal);

        updatePlayerUI();
    }

    public void reload()
    {
        if (gunList.Count == 0 || gunListPos < 0 || gunListPos >= gunList.Count)
            return;

        // How much ammo we can add
        int missingAmmo = gunAmmoCurOriginal[gunListPos] - gunAmmoCur[gunListPos];

        // Make sure session max is enough
        if (missingAmmo > gunAmmoMaxSession[gunListPos])
            missingAmmo = gunAmmoMaxSession[gunListPos];

        if (missingAmmo > 0)
        {
            gunAmmoCur[gunListPos] += missingAmmo;
            gunAmmoMaxSession[gunListPos] -= missingAmmo;

            gamemanager.instance.updateAmmoUI(gunAmmoCur[gunListPos], gunAmmoMaxSession[gunListPos]);

            if (gunList[gunListPos].reloadSound != null)
                aud.PlayOneShot(gunList[gunListPos].reloadSound, gunList[gunListPos].reloadSoundVol);
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
        if (spellslot == null) return;

        Spellstats stats = spellslot.GetSpell(index);
        if (stats == null) return;

        if (index < 0 || index >= spellLoadout.Count) return;

        GameObject spellPrefab = spellLoadout[index];
        if (spellPrefab == null || castPoint == null) return;


        if (manaSystem != null && !manaSystem.UseMana(stats.manaCost))
            return;

        Instantiate(
            spellPrefab,
            castPoint.position,
            castPoint.rotation
        );
    }
    public void UpdateSpellUI(List<Spellstats> spells)
    {
        if (gamemanager.instance != null)
        {
            gamemanager.instance.UpdateSpellUI(spellLoadout);
        }
    }

}