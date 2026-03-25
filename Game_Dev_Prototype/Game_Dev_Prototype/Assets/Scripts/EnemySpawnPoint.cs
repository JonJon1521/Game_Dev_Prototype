using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [Header("Spawn Limit")]
    public int maxEnemies = 5;
    [HideInInspector] public int spawnedEnemies = 0;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f); 
    }
}
