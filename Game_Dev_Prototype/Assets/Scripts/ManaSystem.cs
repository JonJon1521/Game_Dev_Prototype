using UnityEngine;
using UnityEngine.UI;

public class ManaSystem : MonoBehaviour
{
    private StatsManager stats;

    [Header("Current Status")]
    public float currentMana; // Changed to float for smooth regeneration

    void Start()
    {
        stats = GetComponent<StatsManager>();

        // Initialize current mana based on the StatsManager's intelligence calculation
        currentMana = stats.maxMana;
        UpdateUI();
    }

    void Update()
    {
        // --- ADDED: Passive Mana Regeneration ---
        // Regen rate scales with Intelligence (1 mana per sec + 0.5 per Int point)
        if (currentMana < stats.maxMana)
        {
            float regenRate = 1f + (stats.intelligence * 0.5f);
            currentMana += regenRate * Time.deltaTime;

            // Clamp to make sure we don't exceed the limit
            currentMana = Mathf.Clamp(currentMana, 0, stats.maxMana);
            UpdateUI();
        }
    }

    // Changed amount to float to match the currentMana
    public bool UseMana(float amount)
    {
        if (currentMana < amount)
        {
            Debug.Log("Not enough mana for this move!");
            return false;
        }

        currentMana -= amount;
        UpdateUI();
        return true;
    }

    public void RestoreMana(float amount)
    {
        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, stats.maxMana);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (gamemanager.instance != null && gamemanager.instance.playerManaBar != null)
        {
            // Use StatsManager.maxMana for the UI bar
            gamemanager.instance.playerManaBar.fillAmount = currentMana / stats.maxMana;
        }
    }
}