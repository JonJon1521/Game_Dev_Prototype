using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;



public class playerController : MonoBehaviour, IDamage, IPickup
{
    [Header("Compontents")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("Stats")]
    [Range(1, 100)][SerializeField] int HP;
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

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("Audio")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audJump;
    [SerializeField] float audJumpVol;
    [SerializeField] AudioClip[] audStep;
    [SerializeField] float audStepVol;
    [SerializeField] AudioClip[] audHurt;
    [SerializeField] float audHurtVol;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    int jumpCount;
    int HPOriginal;
    int gunListPos;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    float shootTimer;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    bool isPlayingStep;
    bool isSprinting;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    Vector3 moveDir;
    Vector3 playerVel;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    bool isDodging = false;

    public int CurrentGunPos => gunListPos;


    List<int> gunAmmoCur = new List<int>();
    List<int> gunTotalAmmo = new List<int>();
    List<int> gunAmmoMaxOriginal = new List<int>();
    List<int> gunAmmoCurOriginal = new List<int>();
    List<int> gunAmmoMaxSession = new List<int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOriginal = HP;
        spawnPlayer();
        if (gunList.Count > 0)
            gunListPos = 0;


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
        }
    }
    IEnumerator playStep()
    {
        isPlayingStep = true;

        if (aud != null && audStep != null && audStep.Length > 0)
        {
            aud.PlayOneShot(audStep[Random.Range(0, audStep.Length)], audStepVol);
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

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
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
            aud.PlayOneShot(audJump[Random.Range(0, audJump.Length)], audJumpVol);
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
        if (gunList.Count == 0 || gunAmmoCur[gunListPos] <= 0) return;

        gunStats gun = gunList[gunListPos];

        shootTimer = 0;
        gunAmmoCur[gunListPos]--;
        gamemanager.instance.updateAmmoUI(gunAmmoCur[gunListPos], gun.ammoMax);

        // Play gun sound
        if (aud != null && gun.shootSound.Length > 0)
            aud.PlayOneShot(gun.shootSound[Random.Range(0, gun.shootSound.Length)], gun.shootSoundVol);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, gun.shootDist, ~ignoreLayer))
        {
            // Spawn hit effect ONLY when hitting something
            if (gun.hitEffect != null)
            {
                ParticleSystem effect = Instantiate(gun.hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                effect.Play();
                Destroy(effect.gameObject, 0.5f);
            }

            // Deal damage if target implements IDamage
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
        aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);
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

        gunAmmoCur[gunListPos] += amount;

        // Clamp to session max
        if (gunAmmoCur[gunListPos] > gunAmmoMaxSession[gunListPos])
            gunAmmoCur[gunListPos] = gunAmmoMaxSession[gunListPos];

        gamemanager.instance.updateAmmoUI(gunAmmoCur[gunListPos], gunAmmoMaxSession[gunListPos]);
    }
}