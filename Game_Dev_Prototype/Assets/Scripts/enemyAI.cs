using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
//using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;

    [SerializeField] NavMeshAgent agent;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("~~~~~ States ~~~~~")]

    [Range(1, 30)][SerializeField] int HP;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("~~~~~ Gun ~~~~~")]

    [SerializeField] GameObject bullet;

    [SerializeField] float shootRate;

    [SerializeField] Transform shootPos;

    [SerializeField] Transform gunPivot;

    //~~~~~~~~~~~~~~~Ints?~~~~~~~~~~~~~~~~~~~~~

    [SerializeField] int FOV;

    [SerializeField] int faceTargetSpeed;

    [SerializeField] int gunRotateSpeed;

    [SerializeField] int roamPauseTime;

    [SerializeField] int roamDistance;

   

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    Color colorOrig;

    //~~~~~~~~~~~~~~~Floats~~~~~~~~~~~~~~~~~~~~~

    float shootTimer;

    float roamTimer;

    float angleToPlayer;

    float stoppingDistOrig;

    //~~~~~~~~~~~~~~~~Bools~~~~~~~~~~~~~~~~~~~~

    bool playerInRange;
    public bool counted = false;

    //~~~~~~~~~~~~~~~Vectors~~~~~~~~~~~~~~~~~~~~~

    Vector3 playerDir;

    Vector3 startingPos;


   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;

        //gamemanager.instance.updateGameGoal(1);

        startingPos = transform.position;

        stoppingDistOrig = agent.stoppingDistance;

        
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance < 0.1f)
        {
            roamTimer += Time.deltaTime;
        }

        if (playerInRange && !canSeePlayer())
        {
            checkRoam();
        }
        else
        {
            checkRoam();
        }
    }

    void checkRoam()
    {
        if (agent.remainingDistance < 0.1f && roamTimer >= roamPauseTime)
        {
            roam();
        }
    }

    void roam()
    {
        roamTimer = 0;

        agent.stoppingDistance = 0; // makes the enemy stop on that point

        Vector3 ranPos = Random.insideUnitSphere * roamDistance;

        ranPos += startingPos;

        NavMeshHit hit;

        NavMesh.SamplePosition(ranPos, out hit, roamDistance, 1);

        agent.SetDestination(hit.position);
    }

    bool canSeePlayer()
    {
        playerDir = gamemanager.instance.player.transform.position - transform.position;

        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(transform.position, playerDir);

        RaycastHit hit;

        if (Physics.Raycast(transform.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= FOV)
            {
                agent.SetDestination(gamemanager.instance.player.transform.position);

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();
                }

                gunRotate();

                shootTimer += Time.deltaTime;

                if (shootTimer >= shootRate)
                {
                    shoot();
                }

                agent.stoppingDistance = stoppingDistOrig;

                return true;
            }
        }

        agent.stoppingDistance = 0;

        return false;
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);

        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed); // for smoother enemy rotation
    }

    void gunRotate()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);

        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * gunRotateSpeed); // for smoother gun rotation
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            agent.stoppingDistance = 0;
        }
    }

    void shoot()
    {
        shootTimer = 0;

        Instantiate(bullet, shootPos.position, gunPivot.rotation);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        //agent.SetDestination(gameManager.instance.player.transform.position);

        if (HP <= 0)
        {
            // Enemy is dead
            StopAllCoroutines();

            if (counted)
            {
                gamemanager.instance.updateGameGoal(-1);
                counted = false;
            }
               

            Destroy(gameObject);
        }
        else
        {
            if (model != null)
                StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        if (model == null) yield break;

        model.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        if (model != null)
            model.material.color = colorOrig;
    }
}
