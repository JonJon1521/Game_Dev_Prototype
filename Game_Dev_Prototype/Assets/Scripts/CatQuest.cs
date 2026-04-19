using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables; // requierd for cut scenes

public class CatQuest : MonoBehaviour
{
    //~~~~~~~~~~~~~~~~~~~~~~~~~~PUBLIC VARIABLES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    [Header("~~~~Connections~~~~")]

    public EventTracker tracker; // talks to our event tracker

    public GameObject choiceUI; // for the buttons that pop up the choices

    [Header("~~~~Cutscenes~~~~")]

    public PlayableDirector savedCat; // for saved cat cut scene

    public PlayableDirector noSaveCat; // for not saved cat cut scene

    [Header("~~~~UI Promts~~~~")]

    public GameObject interactPrompt;

    //~~~~~~~~~~~~~~~~~~~~~~~~~~PRIVATE VARIABLES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    private bool canInteract;

    private playerController playerScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.F))
        {
            interactPrompt.SetActive(false); // hide the f propmt because we are now talking

            choiceUI.SetActive(true); // turn on the UI buttons on the screen

            Cursor.lockState = CursorLockMode.None; // unlock the mouse from the center of the screen

            Cursor.visible = true; // make the mouse cursor visible so you can click
        }
    }

    public void OnSavedCat()
    {
        choiceUI.SetActive(false); // hide the buttons

        tracker.catSaved = true; // update the "memory" (event tracker)

        tracker.AddPopularity(15); // add to our popularity

        if (playerScript != null) // disabel the player not the girl if its not null but its also just shouldnt disable the girl
        {
            playerScript.enabled = false; // this "freezes" the player so the ther are no camera jitters 
        }

        savedCat.Play(); // plays the cut sceen

        StartCoroutine(EnableControlsAfterDelay((float)savedCat.duration)); // turn on a timer to give back controls when cut scene ends

    }

    public void OnNoSavedCat()
    {
        choiceUI.SetActive(false); // hide the buttons

        tracker.catSaved = false; // update the "memory" (event tracker)

        tracker.AddPopularity(-15); // add to our popularity

        if (playerScript != null) // disable the player not the girl if its not null but it also just shouldnt disable the girls
        {
            playerScript.enabled = false; // this "freezes" the player so the ther are no camera jitters
        }

        noSaveCat.Play(); // plays the cut sceen

        StartCoroutine(EnableControlsAfterDelay((float)noSaveCat.duration)); // turn on a timer to give back controls when cut scene ends

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // check if the player that walked into the circle is tagged as player
        {
            canInteract = true; // allow the e to work

            interactPrompt.SetActive(true); // show the "press e prompt

            playerScript = other.GetComponent<playerController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false; // stop the e key from working

            interactPrompt.SetActive(false); // hide it if you walk away 

            choiceUI.SetActive(false); // close the menu if they walk away
        }
    }

    private void ResetMouse() // helper to put the mouse back to "game mode"
    {
        Cursor.lockState = CursorLockMode.Locked; // snap mouse to center 

        Cursor.visible = false; // hide the mouse cursor
    }

    private IEnumerator EnableControlsAfterDelay(float delay) // to ues in unfreesing the player after cut scene
    {
        yield return new WaitForSeconds(delay); // wait for the length of the cut scene

        if (playerScript != null)
        {
            playerScript.enabled = true; // give control back
        }

        Cursor.lockState = CursorLockMode.Locked; // snap mouse to center 

        Cursor.visible = false; // hide the mouse cursor
    }
}
