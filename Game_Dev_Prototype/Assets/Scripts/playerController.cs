using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;

public class playerController : MonoBehaviour, IDamage
{
  [SerializeField] CharacterController controller;
  [SerializeField] LayerMask ignoreLayer;

  [SerializeField] int HP;
  [SerializeField] int speed;
  [SerializeField] int sprintMod;
  [SerializeField] int jumpSpeed;
  [SerializeField] int jumpMax;
  [SerializeField] int gravity;

  [SerializeField] List<gunStats> gunList = new List<gunStats>();
  [SerializeField] GameObject gunModel;

  [SerializeField] int shootDamage;
  [SerializeField] int shootDist;
  [SerializeField] float shootRate;

  int jumpCount;
  int HPOrig;
  int gunListPos;
  float shootTimer;

  Vector3 moveDir;
  Vector3 playerVel;

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    HPOrig = HP;
    updatePlayerUI();
  }

  // Update is called once per frame
  void Update()
  {
    movement();
    sprint();
  }

  /*public void spawnPlayer()
  {
    controller.transform.position = gameManager.instance.playerSpawnPos.transform.position;
    Physics.SyncTransforms();
    HP = HPOrig;
    updatePlayerUI();
  }*/

  void movement()
  {
    shootTimer += Time.deltaTime;
    Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

    if (controller.isGrounded)
    {
      jumpCount = 0;
      playerVel = Vector3.zero;
    }

    //moveDir = new Vector3(Input.GetAxis("Horizontal") 0, Input.GetAxis("Vertical"));
    moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
    controller.Move(moveDir * speed * Time.deltaTime);

    jump();
    controller.Move(playerVel * Time.deltaTime);
    playerVel.y -= gravity * Time.deltaTime;
    if (Input.GetButton("Fire1") && gunList.Count > 0 && gunList[gunListPos].ammoCur > 0 && shootTimer >= shootRate)
      shoot();
      selectGun();
      reload();
  }

  void jump()
  {
    if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
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
        gunList[gunListPos].ammoCur--;
        //aud.PlayOneShot(gunList[gunListPos].shootSound[Random.Range(0, gunList[gunListPos].shootSound.Length)], gunList[gunListPos].shootSoundVol);
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);
            Instantiate(gunList[gunListPos].hitEffect, hit.point, Quaternion.identity);
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }

        }
    }

  void reload()
  {
    if (Input.GetButtonDown("Reload") && gunList.Count > 0)
      {
        gunList[gunListPos].ammoCur = gunList[gunListPos].ammoMax;
      }
  }

  public void takeDamage(int amount)
  {
    HP -= amount;
    updatePlayerUI();
    StartCoroutine(flashScreen());
    if (HP <= 0)
      {
        gameManager.instance.youLose();
      }
  }

  IEnumerator flashScreen()
  {
    gameManager.instance.playerDamageFlash.SetActive(true);
    yield return new WaitForSeconds(0.1f);
    gameManager.instance.playerDamageFlash.SetActive(false);
  }

  public void updatePlayerUI()
  {
    gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
  }

  public void getGunStats(gunStats gun)
  {
    gunList.Add(gun);
    gunListPos = gunList.Count - 1;
    changeGun();
  }

  void changeGun()
  {
    shootDamage = gunList[gunListPos].shootDamage;
    shootDist = gunList[gunListPos].shootDist;
    shootRate = gunList[gunListPos].shootRate;

    gunModel.GetComponent<MeshFilter>().sharedMesh = gunList[gunListPos].gunModel.GetComponent<MeshFilter>().sharedMesh;
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
}
