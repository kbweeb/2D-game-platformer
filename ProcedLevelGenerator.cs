using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int levelWidth = 100;
    [SerializeField] private int levelHeight = 20;
    [SerializeField] private int groundHeight = 5;
    [SerializeField] private float platformDensity = 0.3f;
    [SerializeField] private int minPlatformLength = 3;
    [SerializeField] private int maxPlatformLength = 8;
    [SerializeField] private int minPlatformHeight = 2;
    [SerializeField] private int maxPlatformHeight = 6;
    [SerializeField] private int minGapWidth = 2;
    [SerializeField] private int maxGapWidth = 5;
    
    [Header("Difficulty Scaling")]
    [SerializeField] private float difficultyMultiplier = 1.0f;
    [SerializeField] private float gapWidthScaling = 0.2f;
    [SerializeField] private float platformDensityScaling = -0.05f;
    
    [Header("Obstacle Settings")]
    [SerializeField] private float spikeFrequency = 0.2f;
    [SerializeField] private float enemyFrequency = 0.15f;
    [SerializeField] private float collectibleFrequency = 0.1f;
    
    [Header("References")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap decorationTilemap;
    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase[] decorationTiles;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private GameObject[] collectiblePrefabs;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform levelParent;
    
    // Level data
    private bool[,] levelData;
    private List<Vector2> spawnPoints = new List<Vector2>();
    private Vector2 playerSpawnPoint;
    
    private void Start()
    {
        GenerateLevel();
    }
    
    public void GenerateLevel(int playerLevel = 1)
    {
        // Clear existing level
        ClearLevel();
        
        // Initialize level data
        levelData = new bool[levelWidth, levelHeight];
        
        // Apply difficulty scaling
        float currentDifficulty = difficultyMultiplier * (1 + (playerLevel - 1) * 0.1f);
        int currentMaxGapWidth = Mathf.Min(maxGapWidth + Mathf.FloorToInt(playerLevel * gapWidthScaling), 8);
        float currentPlatformDensity = Mathf.Max(platformDensity + (playerLevel - 1) * platformDensityScaling, 0.1f);
        
        // Generate ground
        GenerateGround();
        
        // Generate platforms
        GeneratePlatforms(currentPlatformDensity, currentMaxGapWidth);
        
        // Generate obstacles and enemies
        GenerateObstacles(playerLevel);
        
        // Apply tiles to tilemap
        ApplyTilesToTilemap();
        
        // Spawn player
        SpawnPlayer();
        
        // Spawn enemies and collectibles
        SpawnEntities(playerLevel);
    }
    
    private void ClearLevel()
    {
        // Clear tilemaps
        groundTilemap.ClearAllTiles();
        decorationTilemap.ClearAllTiles();
        
        // Clear entities
        if (levelParent != null)
        {
            foreach (Transform child in levelParent)
            {
                Destroy(child.gameObject);
            }
        }
        
        // Clear spawn points
        spawnPoints.Clear();
    }
    
    private void GenerateGround()
    {
        // Generate base ground
        for (int x = 0; x < levelWidth; x++)
        {
            for (int y = 0; y < groundHeight; y++)
            {
                levelData[x, y] = true;
            }
        }
        
        // Add some variation to ground height
        int currentHeight = groundHeight;
        int direction = Random.Range(0, 2) * 2 - 1; // -1 or 1
        
        for (int x = 0; x < levelWidth; x++)
        {
            // Randomly change direction
            if (Random.value < 0.1f)
            {
                direction = Random.Range(0, 2) * 2 - 1;
            }
            
            // Randomly change height
            if (Random.value < 0.2f)
            {
                currentHeight += direction;
                currentHeight = Mathf.Clamp(currentHeight, groundHeight - 2, groundHeight + 2);
            }
            
            // Apply height change
            for (int y = 0; y < currentHeight; y++)
            {
                levelData[x, y] = true;
            }
            
            // Add spawn point on top of ground
            spawnPoints.Add(new Vector2(x, currentHeight));
        }
        
        // Set player spawn point near the beginning
        int spawnX = Random.Range(5, 15);
        playerSpawnPoint = new Vector2(spawnX, GetGroundHeight(spawnX) + 1);
    }
    
    private int GetGroundHeight(int x)
    {
        if (x < 0 || x >= levelWidth)
            return groundHeight;
            
        for (int y = levelHeight - 1; y >= 0; y--)
        {
            if (levelData[x, y])
                return y;
        }
        
        return groundHeight;
    }
    
    private void GeneratePlatforms(float density, int maxGap)
    {
        int x = Random.Range(10, 20);
        
        while (x < levelWidth - 10)
        {
            // Determine platform length
            int platformLength = Random.Range(minPlatformLength, maxPlatformLength + 1);
            
            // Determine platform height
            int platformHeight = Random.Range(minPlatformHeight, maxPlatformHeight + 1) + groundHeight;
            platformHeight = Mathf.Min(platformHeight, levelHeight - 5);
            
            // Create platform
            for (int i = 0; i < platformLength && x + i < levelWidth; i++)
            {
                levelData[x + i, platformHeight] = true;
                
                // Add spawn point on top of platform
                spawnPoints.Add(new Vector2(x + i, platformHeight + 1));
            }
            
            // Move to next platform position
            x += platformLength + Random.Range(minGapWidth, maxGap + 1);
            
            // Randomly adjust height for next platform
            platformHeight += Random.Range(-2, 3);
            platformHeight = Mathf.Clamp(platformHeight, groundHeight + minPlatformHeight, levelHeight - 5);
        }
    }
    
    private void GenerateObstacles(int playerLevel)
    {
        // Add some random obstacles and decorations
        // This will be handled during the entity spawning phase
    }
    
    private void ApplyTilesToTilemap()
    {
        for (int x = 0; x < levelWidth; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                if (levelData[x, y])
                {
                    // Check if this is a top tile
                    bool isTop = y + 1 >= levelHeight || !levelData[x, y + 1];
                    
                    // Apply appropriate tile
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), isTop ? grassTile : groundTile);
                    
                    // Add decoration on top tiles sometimes
                    if (isTop && Random.value < 0.1f && decorationTiles.Length > 0)
                    {
                        TileBase decorTile = decorationTiles[Random.Range(0, decorationTiles.Length)];
                        decorationTilemap.SetTile(new Vector3Int(x, y + 1, 0), decorTile);
                    }
                }
            }
        }
    }
    
    private void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            Instantiate(playerPrefab, playerSpawnPoint, Quaternion.identity, levelParent);
        }
    }
    
    private void SpawnEntities(int playerLevel)
    {
        // Scale frequencies based on player level
        float currentEnemyFrequency = enemyFrequency * (1 + (playerLevel - 1) * 0.05f);
        float currentSpikeFrequency = spikeFrequency * (1 + (playerLevel - 1) * 0.05f);
        
        // Shuffle spawn points for randomness
        ShuffleList(spawnPoints);
        
        // Spawn enemies
        int enemyCount = Mathf.FloorToInt(spawnPoints.Count * currentEnemyFrequency);
        for (int i = 0; i < enemyCount && i < spawnPoints.Count; i++)
        {
            if (Vector2.Distance(spawnPoints[i], playerSpawnPoint) < 10)
                continue; // Don't spawn enemies too close to player start
                
            if (enemyPrefabs.Length > 0)
            {
                GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                GameObject enemy = Instantiate(enemyPrefab, spawnPoints[i], Quaternion.identity, levelParent);
                
                // Set enemy level based on player level
                EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    // Configure enemy based on player level
                    // This would be implemented in the actual game
                }
            }
        }
        
        // Spawn obstacles
        int obstacleCount = Mathf.FloorToInt(spawnPoints.Count * currentSpikeFrequency);
        for (int i = enemyCount; i < enemyCount + obstacleCount && i < spawnPoints.Count; i++)
        {
            if (Vector2.Distance(spawnPoints[i], playerSpawnPoint) < 8)
                continue; // Don't spawn obstacles too close to player start
                
            if (obstaclePrefabs.Length > 0)
            {
                GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                Instantiate(obstaclePrefab, spawnPoints[i], Quaternion.identity, levelParent);
            }
        }
        
        // Spawn collectibles
        int collectibleCount = Mathf.FloorToInt(spawnPoints.Count * collectibleFrequency);
        for (int i = enemyCount + obstacleCount; i < enemyCount + obstacleCount + collectibleCount && i < spawnPoints.Count; i++)
        {
            if (collectiblePrefabs.Length > 0)
            {
                GameObject collectiblePrefab = collectiblePrefabs[Random.Range(0, collectiblePrefabs.Length)];
                Instantiate(collectiblePrefab, spawnPoints[i], Quaternion.identity, levelParent);
            }
        }
    }  spawnPoints[i], Quaternion.identity, levelParent);
            }
        }
    }
    
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
