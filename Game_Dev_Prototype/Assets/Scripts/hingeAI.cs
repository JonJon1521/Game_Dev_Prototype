using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System;

public class hingeAI : MonoBehaviour
{
    [SerializeField] Renderer model;
    [SerializeField] Rigidbody trapDoor;
    [SerializeField] NavMeshObstacle obstacle;
    //[SerializeField] HingeJoint hJoint;
    [SerializeField] float slamRate;
    //[SerializeField] GameObject door;
    [SerializeField]

    Color colorOrig;

    float slamTimer;
    bool playerInRange;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        gameManager.instance.updateGameGoal(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange)
        {
            slamTimer += Time.deltaTime;

            if(slamTimer >= slamRate)
            {
                slam();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    void slam()
    {
        slamTimer = 0;
        HingeJoint(model,trapDoor);
    }

    private void HingeJoint(Renderer model, Rigidbody trapDoor)
    {
        throw new NotImplementedException();
    }
}
