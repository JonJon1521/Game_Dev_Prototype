using System;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    [Header("Leveling System")]
    public int currLevel = 1;
    public int currXP = 0;
    public int skillPoints = 0;
    
    [Header("Core Atrributes")]
    public int strength = 1;
    public int speed = 1;
    public int intelligence = 1;
    public int health = 1;

    [Header("Gameplay Stats")]
    public float moveSpeed;
    public float maxHealth;
    public float maxMana;
    public float carryCapacity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Stats based on starting attributes
        UpdateGameplayStats();
    }

    public void AddXP(int amount)
    {
        currXP += amount;
        if(currXP >= (currLevel + 100))
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
      currXP -= (currLevel *  100);
        currLevel++;
        skillPoints += 2;
    }

    public void UpgradeAttribute(string attributeName)
    {
        if (skillPoints <= 0) return;

        switch (attributeName.ToLower())
        {
            case "strength":
                strength++;
                break;
            case "speed":
                speed++;
                break;
            case "intelligence":
                intelligence++;
                break;
            case "health":
                health++;
                break;
            default:
                Debug.LogWarning("Attribute" + attributeName + "does not exist");
                return;
        }
        skillPoints--;
        UpdateGameplayStats();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateGameplayStats()
    {
        maxHealth = 100f + (health * 10f);
        moveSpeed = 5f + (speed * 0.5f);
        maxMana = 50f + (intelligence * 15f);
        carryCapacity = 20f + (strength * 5f);
    }
}
