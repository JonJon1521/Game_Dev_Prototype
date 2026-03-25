using UnityEngine;

public class Cobwebs : MonoBehaviour
{
    [Range(1, 10)] public int slowSpeed; // a whole number that can be changed in the inspector that will change how much our player is slowed down

    public playerController playerScript; // a veraible to temporaraly hold our playerController script on our player

    private void OnTriggerEnter(Collider other) // runs this as soon as the collider enters the trigger
    {
        if(other.CompareTag("Player")) // check to see if our object toughing is tagged as teh player
        {
            playerScript = other.GetComponent<playerController>(); // reaches into our object thats toughing our web 'other' to find teh playerController script

            if(playerScript != null) // check to see if our script was found 
            {
                playerScript.applySlowSpeed(slowSpeed); // if found run this method in the script with our slowspeed variable
            }
        }
    }
     
    private void OnTriggerExit(Collider other) // runs this as soon as the collider exits the trigger
    {
        if(other.CompareTag("Player")) // check to see if our object toughing is tagged as teh player
        {
            playerScript = other.GetComponent<playerController>(); // reaches into our object thats toughing our web 'other' to find teh playerController script

            if (playerScript != null) // check to see if our script was found
            {
                playerScript.removeSlowSpeed(slowSpeed); // if found run this method in the script with our slowspeed variable
            }
        }
    }
}
