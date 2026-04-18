using UnityEngine;

public class Endgoal : MonoBehaviour
{
    int requiredCollectibles;
    bool playerInRange;

    void Start()
    {
        requiredCollectibles =
            Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None).Length;

        gamemanager.instance.SetRequiredCollectibles(requiredCollectibles);
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (gamemanager.instance.IsGoalUnlocked())
            {
                Debug.Log("You Win! Exit Used");
                // put win logic here
            }
            else
            {
                Debug.Log("Exit Locked - Collect everything first");
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
}
