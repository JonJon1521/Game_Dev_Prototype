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

    [Header("Optional: Player Proximity")]
    public Transform player;                
    public float spawnDistance = 30f;       
    public bool checkPlayerDistance = false; 

    private float spawnTimer;
    private int nextIndex = 0;

    void Start()
    {
        //SpawnEnemy();

        enemyAI[] manualEnemies = GameObject.FindObjectsOfType<enemyAI>();
        foreach (enemyAI e in manualEnemies)
        {
            gameManager.instance.updateGameGoal(1);
        }

        if (GameObject.FindObjectsOfType<enemyAI>().Length == 0)
        {
            SpawnEnemy();
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

        EnemySpawnPoint spawnPointScript;
        Transform spawnPointTransform;

        if (spawnRandom)
        {
            int tries = 0;
            do
            {
                spawnPointScript = spawnPoints[Random.Range(0, spawnPoints.Length)];
                tries++;
            } while (spawnPointScript.spawnedEnemies >= spawnPointScript.maxEnemies && tries < 10);

            // If all spawn points reached max, do nothing
            if (spawnPointScript.spawnedEnemies >= spawnPointScript.maxEnemies) return;

            spawnPointTransform = spawnPointScript.transform;
        }
        else
        {
            spawnPointScript = spawnPoints[nextIndex];
            spawnPointTransform = spawnPointScript.transform;
            nextIndex = (nextIndex + 1) % spawnPoints.Length;

            if (spawnPointScript.spawnedEnemies >= spawnPointScript.maxEnemies) return;
        }

        // Optional: check player distance
        if (checkPlayerDistance && Vector3.Distance(player.position, spawnPointTransform.position) > spawnDistance)
            return;

        // Add spawn offset
        Vector3 spawnPos = spawnPointTransform.position + new Vector3(
            Random.Range(-spawnOffset, spawnOffset),
            0f,
            Random.Range(-spawnOffset, spawnOffset)
        );

        // Spawn enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, spawnPointTransform.rotation);
        enemy.transform.parent = transform;

        // Update game manager
        gameManager.instance.updateGameGoal(1);

        // Increment spawn point counter
        spawnPointScript.spawnedEnemies++;
    }

}