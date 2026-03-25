using UnityEngine;

public class ammoPickup : MonoBehaviour
{
    [Range(1, 100)] public int ammoAmount;

    public playerController playerScript;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerScript = other.GetComponent<playerController>();

            if (playerScript != null)
            {
                playerScript.refill(ammoAmount);

                Destroy(gameObject);
            }
        }
    }
}