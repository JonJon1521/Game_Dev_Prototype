using UnityEngine;

public class Endgoal : MonoBehaviour
{
    bool playerInRange;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.L))
        {
            if (gamemanager.instance.IsGoalUnlocked())
            {
                gamemanager.instance.ShowWinMenu();
            }
            else
            {
                Debug.Log("Exit Locked");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}