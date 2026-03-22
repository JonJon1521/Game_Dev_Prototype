using UnityEngine;
using System.Collections.Generic;
using System.Collections;



public class playerController : MonoBehaviour, IDamage, IPickup
{
    [Header("Compontents")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("Stats")]
    [Range(1, 10)][SerializeField] int HP;
    [Range(1, 10)][SerializeField] int speed;
    [Range(2, 6)][SerializeField] int sprintMod;
    [Range(5, 25)][SerializeField] int jumpSpeed;
    [Range(1, 4)][SerializeField] int jumptimesMax;
    [Range(15, 50)][SerializeField] int gravity;


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


    int jumpCount;
    int HPOriginal;
    int gunListPos;



    float shootTimer;

    bool isPlayingStep;
    bool isSprinting;

    Vector3 moveDir;
    Vector3 playerVel;


    bool isDodging = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOriginal = HP;
        spawnPlayer();



    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.instance.isPaused)
            movement();
        sprint();
        HandleDodgeInput();
    }
    IEnumerator playStep()
    {
        isPlayingStep = true;
        aud.PlayOneShot(audStep[Random.Range(0, audStep.Length)], audStepVol);

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
        controller.transform.position = gameManager.instance.playerSpawnPos.transform.position;
        Physics.SyncTransforms();
        HP = HPOriginal;
        updatePlayerUI();

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


        if (Input.GetButtonDown("Fire1") && gunList.Count > 0 && gunList[gunListPos].ammoCur > 0 && shootTimer >= gunList[gunListPos].shootRate)
        {
            shoot();
        }

        selectGun();

        if (moveDir.normalized.magnitude > 0.3f && !isPlayingStep)
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

        shootTimer = 0;

        gunList[gunListPos].ammoCur--;
        aud.PlayOneShot(gunList[gunListPos].shootSound[Random.Range(0, gunList[gunListPos].shootSound.Length)], gunList[gunListPos].shootSoundVol);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, gunList[gunListPos].shootDist, ~ignoreLayer))
        {
            if (gunList[gunListPos].hitEffect != null)

                Instantiate(gunList[gunListPos].hitEffect, hit.point, Quaternion.LookRotation(hit.normal));

            Debug.Log(hit.collider.name);
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(gunList[gunListPos].shootDamage);
            }
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

    public void TakeDamage(int damage)
    {
        HP -= damage;
        updatePlayerUI();
        aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);
        StartCoroutine(flashDamage());
        if (HP <= 0)
        {
            // Fully qualify if needed
            global::gameManager.instance.youLose();
        }
    }
    IEnumerator flashDamage()
    {
        gameManager.instance.damagePlayerFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.damagePlayerFlash.SetActive(false);
    }



    public void updatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOriginal;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOriginal;

        if (HP <= 0)
        {
            gameManager.instance.youLose();
        }
    }
    public void getGunStats(gunStats gun)
    {
        gunList.Add(gun);
        gunListPos = gunList.Count - 1;
        changeGun();
    }

    void changeGun()
    {
        gunModel.GetComponent<MeshFilter>().mesh = gunList[gunListPos].gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunList[gunListPos].gunModel.GetComponent<MeshRenderer>().sharedMaterial;
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
        speed += amount;
    }
    public void applySlowSpeed(int amount)
    {
        speed -= amount;
    }

}