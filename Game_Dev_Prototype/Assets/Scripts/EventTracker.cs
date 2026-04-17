using UnityEngine;

[CreateAssetMenu(fileName = "WorldState", menuName = "Game/Event Tracker")]

public class EventTracker : ScriptableObject
{

    public bool catSaved;

    public int popularity;

   public void AddPopularity(int amount)
    {
        popularity += amount;

        Debug.Log("Popularity is now:" + popularity);
    }

    public void ResetGame()
    {
        catSaved = false;

        popularity = 100;
    }
}
