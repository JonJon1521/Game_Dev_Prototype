using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;



public class playerController : MonoBehaviour, IDamage
{
    [Header("Compontents")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("Stats")]
    [Range(1, 1000)][SerializeField] int HP;
    [Range(1, 10)][SerializeField] int speed;
    [Range(2, 6)][SerializeField] int sprintMod;
    [Range(5, 25)][SerializeField] int jumpSpeed;
    [Range(1, 4)][SerializeField] int jumptimesMax;
    [Range(15, 50)][SerializeField] int gravity;

    [Header("Dodge Settings")]
    [SerializeField] float dodgeDistance = 3f;
    [SerializeField] float dodgeSpeed = 8f;

    [Header("Guns")]
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;

    int jumpCount;
    int HPOriginal;

    float shootTimer;

    Vector3 moveDir;
    Vector3 playerVel;

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


        if (Input.GetButtonDown("Fire1") && shootTimer >= shootRate)
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
        shootTimer = 0;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
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
}