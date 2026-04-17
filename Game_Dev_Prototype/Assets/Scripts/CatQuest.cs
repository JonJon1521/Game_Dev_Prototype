using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class CatQuest : MonoBehaviour
{
    public EventTracker tracker; // talks to our event tracker

    public GameObject choiceUI; // for the buttons that pop up the choices

    public PlayableDirector savedCat; // for saved cat cut scene

    public PlayableDirector noSaveCat; // for not saved cat cut scene

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSavedCat()
    {
        choiceUI.SetActive(false); // hide the buttons

        tracker.catSaved = true; // update the "memory" (event tracker)

        tracker.AddPopularity(15); // add to our popularity

        savedCat.Play();
    }

    public void OnNoSavedCat()
    {
        choiceUI.SetActive(false);

        tracker.catSaved = false;

        tracker.AddPopularity(-15);

        noSaveCat.Play();

    }
     
}
