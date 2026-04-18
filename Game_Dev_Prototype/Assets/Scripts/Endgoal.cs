using UnityEngine;

public class Endgoal : MonoBehaviour
{
    int requiredCollectibles;
    int amount;
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
                gamemanager.instance.ShowWinMenu();
            }
            else
            {
                Debug.Log("Exit Locked");
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
