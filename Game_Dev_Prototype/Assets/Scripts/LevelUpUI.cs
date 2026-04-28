using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelUpUIScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject uiPanel;
    private StatsManager playerStats;

    [Header("UI Text Elements")]
    [SerializeField] private TMP_Text pointsRemainingText;
    [SerializeField] private TMP_Text strText;
    [SerializeField] private TMP_Text spdText;
    [SerializeField] private TMP_Text intText;
    [SerializeField] private TMP_Text hpText;

    void Update()
    {
        // 'P' key to toggle the menu
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        // active state of the panel
        bool isOpening = !uiPanel.activeSelf;
        uiPanel.SetActive(isOpening);

        if (isOpening)
        {
            // find the Player and their StatsManager
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerStats = player.GetComponent<StatsManager>();
                UpdateUIValues(); // Refresh numbers immediately
                Debug.Log("Level Up Menu Opened: Player Found.");
            }
            else
            {
                Debug.LogError("Level Up Menu: NO PLAYER FOUND! Check the 'Player' tag.");
            }
        }
    }

    // This is the function linked to your [ + ] Buttons
    public void Upgrade(string statName)
    {
        Debug.Log("THE BUTTON WAS PRESSED! Stat requested: " + statName);
    
        // Safety Check: for player reference, try to find it one last time
        if (playerStats == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerStats = player.GetComponent<StatsManager>();
        }

        if (playerStats != null)
        {
            if (playerStats.skillPoints > 0)
            {
                Debug.Log("Attempting to upgrade: " + statName);

                playerStats.UpgradeAttribute(statName);

                // Refresh UI text so the player sees the change
                UpdateUIValues();
            }
            else
            {
                Debug.LogWarning("Cannot upgrade: 0 skill points remaining.");
            }
        }
    }

    public void UpdateUIValues()
    {
        if (playerStats == null) return;

        // Update all the text fields with the latest numbers from StatsManager
        pointsRemainingText.text = "Skill Points: " + playerStats.skillPoints;
        strText.text = playerStats.strength.ToString();
        spdText.text = playerStats.speed.ToString();
        intText.text = playerStats.intelligence.ToString();
        hpText.text = playerStats.health.ToString();
    }
}
