using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu]
public class Spellstats : ScriptableObject
{
    public GameObject Spell;
    [Range(0, 100)] public int manaCost;

    [Range(3, 15)] public int shootDamage;
    [Range(5, 1000)] public int shootDist;
    [Range(0, 20)] public float shootRate;

    public ParticleSystem hitEffect;
}
