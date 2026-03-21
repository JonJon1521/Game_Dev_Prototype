using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Setup")]
    public GameObject enemyPrefab;

    [Header("Spawn Points")]
    public EnemySpawnPoint[] spawnPoints;

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public bool spawnRandom = true;
    public float spawnOffset = 1f;

    [Header("Manual Enemy")]
    public enemyAI manualEnemy;

    [Header("Optional: Player Proximity")]
    public Transform player;                
    public float spawnDistance = 30f;       
    public bool checkPlayerDistance = false;

    [Header("Initial Maze Spawns")]
    public int initialEnemiesPerPoint = 3;

    private float spawnTimer;
    private int nextIndex = 0;

    void Start()
    {
        // Count manual enemy
        if (manualEnemy != null && !manualEnemy.counted)
        {
            gameManager.instance.updateGameGoal(1);
            manualEnemy.counted = true;
        }


        // Spawn initial maze enemies
        foreach (EnemySpawnPoint sp in spawnPoints)
        {
            int spawnCount = Mathf.Min(initialEnemiesPerPoint, sp.maxEnemies);
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnEnemyAt(sp);
            }
        }
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;

        EnemySpawnPoint sp;
        Transform spTransform;

        if (spawnRandom)
        {
            int tries = 0;
            do
            {
                sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                tries++;
            } while ((sp.spawnedEnemies >= sp.maxEnemies || !PlayerIsNear(sp)) && tries < 10);

            if (sp.spawnedEnemies >= sp.maxEnemies || !PlayerIsNear(sp))
                return;

            spTransform = sp.transform;
        }
        else
        {
            sp = spawnPoints[nextIndex];
            spTransform = sp.transform;
            nextIndex = (nextIndex + 1) % spawnPoints.Length;

            if (sp.spawnedEnemies >= sp.maxEnemies || !PlayerIsNear(sp))
                return;
        }

        SpawnEnemyAt(sp);
    }

    bool PlayerIsNear(EnemySpawnPoint sp)
    {
        if (!checkPlayerDistance) return true;   // ignore distance if disabled
        if (player == null) return true;         // fallback
        return Vector3.Distance(player.position, sp.transform.position) <= spawnDistance;
    }

    void SpawnEnemyAt(EnemySpawnPoint sp)
    {
        Vector3 spawnPos = sp.transform.position + new Vector3(
            Random.Range(-spawnOffset, spawnOffset),
            0f,
            Random.Range(-spawnOffset, spawnOffset)
        );

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, sp.transform.rotation);
        enemy.transform.parent = transform;

        // Increment game manager
        enemyAI ai = enemy.GetComponent<enemyAI>();
        if (ai != null && !ai.counted)
        {
            gameManager.instance.updateGameGoal(1);
            ai.counted = true; // mark this instance as counted
        }

        // Increment spawn point counter
        sp.spawnedEnemies++;
    }

}