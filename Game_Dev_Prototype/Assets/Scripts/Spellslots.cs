using UnityEngine;
using System.Collections.Generic;

public class Spellslot : MonoBehaviour
{
    [SerializeField] private List<Spellstats> equippedSpells;

    public void EquipSpell(int slotIndex, Spellstats spell)
    {
        if (slotIndex < 0 || slotIndex >= equippedSpells.Count) return;

        equippedSpells[slotIndex] = spell;
    }

    public Spellstats GetSpell(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSpells.Count) return null;

        return equippedSpells[slotIndex];
    }
}