using UnityEngine;

public class Collectible : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        gamemanager.instance.AddCollectible();
        Destroy(gameObject);
    }
}