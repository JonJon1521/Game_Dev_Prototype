using UnityEngine;
using TMPro; // Use TextMeshPro for better looking text
using UnityEngine.UI;

public class LevelUpUIScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] StatsManager stats;
    [SerializeField] GameObject uiPanel;

    [Header("Text Elements")]
    [SerializeField] TMP_Text pointsRemainingText;
    [SerializeField] TMP_Text strText, spdText, intText, hpText;

    bool isMenuOpen = false;

    void Update()
    {
        // Press 'P' to open the Level Up menu
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        uiPanel.SetActive(isMenuOpen);

        if (isMenuOpen)
        {
            // tell the GameManager we are paused so it stops locking the mouse
            if (gamemanager.instance != null) gamemanager.instance.isPaused = true;

            UpdateUIValues();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
        }
        else
        {
            // tell the GameManager we are back in action
            if (gamemanager.instance != null) gamemanager.instance.isPaused = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
        }
    }

    public void Upgrade(string statName)
    {
        stats.UpgradeAttribute(statName);
        UpdateUIValues();
    }

    void UpdateUIValues()
    {
        pointsRemainingText.text = "Skill Points: " + stats.skillPoints;
        strText.text = stats.strength.ToString();
        spdText.text = stats.speed.ToString();
        intText.text = stats.intelligence.ToString();
        hpText.text = stats.health.ToString();
    }
}
