using UnityEngine;

public class ManaPotion : MonoBehaviour
{
    [Range(1, 1000)] public int manaAmount; // a whole number that can be changed in the inspector that will change how much mana our player gets

    public playerController playerScript; // a veraible to temporaraly hold our playerController script on our player

    public bool canPickUp = false;

    public GameObject interactPrompt;

     void Update()
    {
        if ( canPickUp && Input.GetKeyDown(KeyCode.E))
        {
            pickUP();

        }
    }

    private void OnTriggerEnter(Collider other) // runs this as soon as the collider enters the trigger
    {
        if (other.CompareTag("Player")) // check to see if our object toughing is tagged as the player
        {
            playerScript = other.GetComponent<playerController>(); // reaches into our object thats toughing our healthkit 'other' to find the playerController script

            if (playerScript != null) // check to see if our script was found 
            {
                canPickUp = true; // player is in range but dont pick up yet

                if(interactPrompt != null)
                {
                    interactPrompt.SetActive(true); // so the plaeyr sees the prompt 
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // check to see if our object toughing is tagged as the player
        {
            canPickUp = false; // player walked away disable the button check

            playerScript = null;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false); // so the player dosent sees the prompt when waling away
            }
        }
    }

   void pickUP()
    {
        playerScript.restoreMana(manaAmount); // if found run this method in the script with our mana variable

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false); // so the player dosent sees the prompt once its picked up 
        }

        Destroy(gameObject); // then destroy the game object so its not still there after
    }
}
