using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    [SerializeField] private int playerLives = 3;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float levelCompletionTime = 180f;
    
    [Header("References")]
    [SerializeField] private ProceduralLevelGenerator levelGenerator;
    [SerializeField] private UIManager uiManager;
    
    // Game state
    private int currentScore = 0;
    private int collectiblesCollected = 0;
    private int enemiesDefeated = 0;
    private float levelTimer = 0f;
    private bool isGamePaused = false;
    private bool isGameOver = false;
    private bool isLevelComplete = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        StartLevel(currentLevel);
    }
    
    private void Update()
    {
        if (isGamePaused || isGameOver || isLevelComplete)
            return;
            
        // Update level timer
        levelTimer += Time.deltaTime;
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateTimer(levelCompletionTime - levelTimer);
        }
        
        // Check for level completion
        if (levelTimer >= levelCompletionTime)
        {
            CompleteLevel();
        }
        
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    public void StartLevel(int level)
    {
        // Reset level state
        currentLevel = level;
        levelTimer = 0f;
        isGameOver = false;
        isLevelComplete = false;
        
        // Generate level
        if (levelGenerator != null)
        {
            levelGenerator.GenerateLevel(level);
        }
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateLevelText(level);
            uiManager.UpdateLives(playerLives);
            uiManager.UpdateScore(currentScore);
        }
        
        // Start level music
        AudioManager.Instance?.PlayMusic("level_" + (level % 3 + 1));
    }
    
    public void CompleteLevel()
    {
        if (isLevelComplete)
            return;
            
        isLevelComplete = true;
        
        // Calculate bonus points
        int timeBonus = Mathf.FloorToInt((levelCompletionTime - levelTimer) * 10);
        int levelBonus = currentLevel * 100;
        int totalBonus = timeBonus + levelBonus;
        
        // Add bonus to score
        AddScore(totalBonus);
        
        // Show level complete UI
        if (uiManager != null)
        {
            uiManager.ShowLevelComplete(currentLevel, timeBonus, levelBonus, collectiblesCollected, enemiesDefeated);
        }
        
        // Play level complete sound
        AudioManager.Instance?.PlaySound("level_complete");
        
        // Proceed to next level after delay
        StartCoroutine(NextLevelDelay());
    }
    
    private IEnumerator NextLevelDelay()
    {
        yield return new WaitForSeconds(3f);
        
        // Increment level
        currentLevel++;
        
        // Start next level
        StartLevel(currentLevel);
    }
    
    public void GameOver()
    {
        if (isGameOver)
            return;
            
        isGameOver = true;
        
        // Show game over UI
        if (uiManager != null)
        {
            uiManager.ShowGameOver(currentScore);
        }
        
        // Play game over sound
        AudioManager.Instance?.PlaySound("game_over");
    }
    
    public void RestartGame()
    {
        // Reset game state
        currentLevel = 1;
        currentScore = 0;
        playerLives = 3;
        collectiblesCollected = 0;
        enemiesDefeated = 0;
        
        // Start first level
        StartLevel(currentLevel);
    }
    
    public void PlayerDied()
    {
        playerLives--;
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateLives(playerLives);
        }
        
        // Check for game over
        if (playerLives <= 0)
        {
            GameOver();
        }
        else
        {
            // Respawn player after delay
            StartCoroutine(RespawnPlayerDelay());
        }
    }
    
    private IEnumerator RespawnPlayerDelay()
    {
        yield return new WaitForSeconds(2f);
        
        // Respawn player
        // This would be handled by the player controller in a real implementation
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateScore(currentScore);
        }
    }
    
    public void CollectibleCollected()
    {
        collectiblesCollected++;
        AddScore(50);
        
        // Play sound
        AudioManager.Instance?.PlaySound("collectible");
    }
    
    public void EnemyDefeated()
    {
        enemiesDefeated++;
        AddScore(100);
        
        // Play sound
        AudioManager.Instance?.PlaySound("enemy_defeat");
    }
    
    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        
        // Pause/unpause game
        Time.timeScale = isGamePaused ? 0f : 1f;
        
        // Show/hide pause menu
        if (uiManager != null)
        {
            uiManager.TogglePauseMenu(isGamePaused);
        }
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
