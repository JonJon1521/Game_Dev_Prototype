using UnityEngine;

public class Collectible : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        gamemanager.instance.updateGameGoal(-1);
        Destroy(gameObject);
    }
}