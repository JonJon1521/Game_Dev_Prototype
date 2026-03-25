using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class playerController : MonoBehaviour, IDamage
{
    [Header("Compontents")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("Stats")]
    [Range(1, 1000)][SerializeField] int HP;
    [Range(1, 10)][SerializeField] int speed;
    [Range(2, 6)][SerializeField] int sprintMod;
    [Range(5, 25)][SerializeField] int jumpSpeed;
    [Range(1, 4)][SerializeField] int jumptimesMax;
    [Range(15, 50)][SerializeField] int gravity;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("Dodge Settings")]
    [SerializeField] float dodgeDistance = 3f;
    [SerializeField] float dodgeSpeed = 8f;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("Guns")]

    [SerializeField] List<gunStats> gunList = new List<gunStats>();

    [SerializeField] GameObject gunModel;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("~~~~~~~ Audio ~~~~~~~~")]

    [SerializeField] AudioSource aud;

    [SerializeField] AudioClip[] audJump;

    [SerializeField] float audJumpVol;

    [SerializeField] AudioClip[] audHurt;

    [SerializeField] float audHurtVol;

    [SerializeField] AudioClip[] audStep;

    [SerializeField] float audStepVol;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    int jumpCount;
    int HPOriginal;

    int gunListPos;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    float shootTimer;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    Vector3 moveDir;
    Vector3 playerVel;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    bool isDodging = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOriginal = HP;
        updatePlayerUI();

    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
        HandleDodgeInput();
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
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumptimesMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }

    void shoot()
    {
        gunList[gunListPos].ammoCur--;

        aud.PlayOneShot(gunList[gunListPos].shootSound[Random.Range(0, gunList[gunListPos].shootSound.Length)], gunList[gunListPos].shootSoundVol);

        shootTimer = 0;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, gunList[gunListPos].shootDist, ~ignoreLayer))
        {
            if (gunList[gunListPos].hitEffect)
            {
                Instantiate(gunList[gunListPos].hitEffect, hit.point, Quaternion.identity);
            }

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
        if (HP <= 0)
        {
            // Fully qualify if needed
            global::gameManager.instance.youLose();
        }
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

    public void applySlowSpeed(int amount)
    {
        speed = speed / amount; // takes our currents speed at what its set to and divides the amount 
    }

    public void removeSlowSpeed(int amount)
    {
        speed = speed * amount; // takes our current (slow) speed and adds the 'amount' back to it
    }

    public void heal(int amount) // for health kits 
    {
        HP += amount; // we want to add the amount to our health

        if(HP > HPOriginal) // keeps the HP from going over the original hp
        {
            HP = HPOriginal; // then we want our HP to be equal to our Original HP
        }

        gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOriginal; // have to update the HP bar 

        Debug.Log("Healed! HP:" + HP);
    }

}