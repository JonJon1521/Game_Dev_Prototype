using UnityEngine;

public class HealthKit : MonoBehaviour
{
   [Range(1, 1000)] public int health; // a whole number that can be changed in the inspector that will change how much health our player gets

    public playerController playerScript; // a veraible to temporaraly hold our playerController script on our player

    private void OnTriggerEnter(Collider other) // runs this as soon as the collider enters the trigger
    {
        if(other.CompareTag("Player")) // check to see if our object toughing is tagged as teh player
        {
            playerScript = other.GetComponent<playerController>(); // reaches into our object thats toughing our healthkit 'other' to find the playerController script

            if (playerScript != null) // check to see if our script was found 
            {
                playerScript.heal(health); // if found run this method in the script with our health variable

                Destroy(gameObject); // then destroy the game object so its not still there after
            }
        }
    }
}
