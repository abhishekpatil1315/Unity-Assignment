using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GameManager : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ObstacleManager obstacleManager;
    [SerializeField] private Pathfinding pathfinding;
    [SerializeField] private PlayerController player;
    [SerializeField] private EnemyAI enemy;
    
    [Header("Game Settings")]
    [SerializeField] private bool enableDebugLog = true;
    
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (obstacleManager == null) obstacleManager = FindFirstObjectByType<ObstacleManager>();
        if (pathfinding == null) pathfinding = FindFirstObjectByType<Pathfinding>();
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (enemy == null) enemy = FindFirstObjectByType<EnemyAI>();
        
        LogStatus("Game Manager initialized successfully!");
    }
    
    void Update()
    {
        if (GetKeyDown(KeyCode.R))
        {
            RefreshObstacles();
        }
        
        if (GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }
    
    /// <summary>
    /// Refresh all obstacles from the ObstacleData
    /// Useful for runtime testing
    /// </summary>
    public void RefreshObstacles()
    {
        if (obstacleManager != null)
        {
            obstacleManager.RefreshObstacles();
            LogStatus("Obstacles refreshed!");
        }
    }
    
    /// <summary>
    /// Reset player to starting position
    /// </summary>
    public void ResetPlayer()
    {
        if (player != null)
        {
            player.StopMovement();
            player.InitializePosition(0, 0);
            LogStatus("Player reset to starting position!");
        }
    }
    
    /// <summary>
    /// Quit the game (works in build)
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Log status message if debug logging is enabled
    /// </summary>
    private void LogStatus(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[GameManager] {message}");
        }
    }
    
    /// <summary>
    /// Get reference to the player
    /// </summary>
    public PlayerController GetPlayer()
    {
        return player;
    }
    
    /// <summary>
    /// Get reference to the enemy
    /// </summary>
    public EnemyAI GetEnemy()
    {
        return enemy;
    }
    
    /// <summary>
    /// Get key down supporting both old and new Input System
    /// </summary>
    private bool GetKeyDown(KeyCode key)
    {
        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            Key newKey = key switch
            {
                KeyCode.R => Key.R,
                KeyCode.Escape => Key.Escape,
                _ => Key.None
            };
            if (newKey != Key.None)
            {
                return Keyboard.current[newKey].wasPressedThisFrame;
            }
        }
        #endif
        return Input.GetKeyDown(key);
    }
}
