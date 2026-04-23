using UnityEngine;
using UnityEngine.UI;

public class ManaSystem : MonoBehaviour
{
    [SerializeField] int maxMana = 100;
    [SerializeField] int currentMana;



    void Start()
    {
        currentMana = maxMana;
        UpdateUI();
    }

    public bool UseMana(int amount)
    {
        if (currentMana < amount)
            return false;

        currentMana -= amount;
        UpdateUI();
        return true;
    }

    public void RestoreMana(int amount)
    {
        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (gamemanager.instance != null && gamemanager.instance.playerManaBar != null)
        {
            gamemanager.instance.playerManaBar.fillAmount =
                (float)currentMana / maxMana;
        }
    }
}