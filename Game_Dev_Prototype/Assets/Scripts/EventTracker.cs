using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "WorldState", menuName = "Game/Event Tracker")]

public class EventTracker : ScriptableObject
{

    public bool catSaved; // log for cat quest 

    public int popularity; // log for popularity

    public UnityEvent<int> onPopularityChanged;

   public void AddPopularity(int amount) 
    {
        popularity = Mathf.Clamp(popularity + amount, 0, 100);

        onPopularityChanged?.Invoke(popularity);

        Debug.Log("Popularity is now:" + popularity);
    }

    public void ResetGame() // for new games 
    {
        catSaved = false;

        popularity = 50;
    }
}
