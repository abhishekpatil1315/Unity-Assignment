using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour, IAI
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float heightAboveGround = 0.5f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color enemyColor = Color.red;
    
    [Header("AI Behavior")]
    [SerializeField] private bool autoTakeTurn = true;
    
    private int currentGridX;
    private int currentGridY;
    
    private GridManager gridManager;
    private Pathfinding pathfinding;
    private PlayerController player;
    private Animator animator;
    
    private bool isTakingTurn = false;
    private Vector2Int lastKnownPlayerPosition;
    
    void Start()
    {
        gridManager = GridManager.Instance;
        pathfinding = Pathfinding.Instance;
        player = FindFirstObjectByType<PlayerController>();
        
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found! EnemyAI requires GridManager.");
            return;
        }
        
        if (player == null)
        {
            Debug.LogError("PlayerController not found! EnemyAI requires a player to follow.");
            return;
        }
        
        SetupVisuals();
        animator = GetComponent<Animator>();
        StartCoroutine(WaitForGridAndInitialize());
    }
    
    private IEnumerator WaitForGridAndInitialize()
    {
        while (gridManager.GetAllTiles() == null)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.2f);
        Initialize(4, 9);
        lastKnownPlayerPosition = player.CurrentGridPosition;
    }
    
    void Update()
    {
        if (autoTakeTurn && !isTakingTurn && player != null)
        {
            Vector2Int currentPlayerPosition = player.CurrentGridPosition;
            
            if (!player.IsMoving && currentPlayerPosition != lastKnownPlayerPosition)
            {
                lastKnownPlayerPosition = currentPlayerPosition;
                TakeTurn();
            }
        }
    }
    
    private void SetupVisuals()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (renderer.material != null)
            {
                renderer.material.color = enemyColor;
            }
            else
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                renderer.material.color = enemyColor;
            }
        }
    }
    
    /// <summary>
    /// Initialize enemy position at specific grid coordinates
    /// Implementation of IAI interface
    /// </summary>
    public void Initialize(int gridX, int gridY)
    {
        if (gridManager == null || !gridManager.IsValidGridPosition(gridX, gridY))
        {
            Debug.LogWarning("Invalid initialization position for Enemy!");
            return;
        }
        
        currentGridX = gridX;
        currentGridY = gridY;
        
        GridTile tile = gridManager.GetTile(gridX, gridY);
        if (tile != null)
        {
            Vector3 targetPosition = tile.WorldPosition + Vector3.up * heightAboveGround;
            transform.position = targetPosition;
        }
    }
    
    /// <summary>
    /// Enemy takes its turn - moves closer to the player
    /// Implementation of IAI interface
    /// </summary>
    public void TakeTurn()
    {
        if (isTakingTurn || player == null || pathfinding == null)
        {
            return;
        }
        
        // Get player's current position
        Vector2Int playerPos = player.CurrentGridPosition;
        
        // Calculate target position (adjacent to player)
        Vector2Int targetPos = FindBestAdjacentTile(playerPos);
        
        // If already adjacent to player, don't move
        if (IsAdjacentToPlayer(currentGridX, currentGridY, playerPos.x, playerPos.y))
        {
            Debug.Log("Enemy is already adjacent to player. Waiting...");
            return;
        }
        
        // Move to the target position
        if (targetPos.x != -1 && targetPos.y != -1)
        {
            StartCoroutine(MoveToPosition(targetPos.x, targetPos.y));
        }
    }
    
    /// <summary>
    /// Find the best adjacent tile to the player that the enemy can reach
    /// Prioritizes tiles that are closer to the enemy's current position
    /// </summary>
    private Vector2Int FindBestAdjacentTile(Vector2Int playerPosition)
    {
        // Four adjacent positions (up, down, left, right)
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };
        
        Vector2Int bestTarget = new Vector2Int(-1, -1);
        int shortestPathLength = int.MaxValue;
        
        // Check each adjacent tile
        for (int i = 0; i < 4; i++)
        {
            int targetX = playerPosition.x + dx[i];
            int targetY = playerPosition.y + dy[i];
            
            // Check if position is valid and walkable
            if (gridManager.IsValidGridPosition(targetX, targetY))
            {
                GridTile tile = gridManager.GetTile(targetX, targetY);
                
                if (tile != null && tile.IsWalkable())
                {
                    // Find path to this adjacent tile
                    List<GridTile> path = pathfinding.FindPath(
                        currentGridX, currentGridY, 
                        targetX, targetY
                    );
                    
                    // Choose the tile with the shortest path
                    if (path != null && path.Count < shortestPathLength)
                    {
                        shortestPathLength = path.Count;
                        bestTarget = new Vector2Int(targetX, targetY);
                    }
                }
            }
        }
        
        return bestTarget;
    }
    
    /// <summary>
    /// Check if a position is adjacent to the player (4-directional)
    /// </summary>
    private bool IsAdjacentToPlayer(int x, int y, int playerX, int playerY)
    {
        int dx = Mathf.Abs(x - playerX);
        int dy = Mathf.Abs(y - playerY);
        
        // Adjacent means one tile away in one direction only
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }
    
    /// <summary>
    /// Coroutine to move the enemy to a target position
    /// </summary>
    private IEnumerator MoveToPosition(int targetX, int targetY)
    {
        isTakingTurn = true;
        if (animator != null) animator.SetBool("isWalking", true);
        
        // Find path to target
        List<GridTile> path = pathfinding.FindPath(currentGridX, currentGridY, targetX, targetY);
        
        if (path == null || path.Count == 0)
        {
            Debug.Log("Enemy cannot find path to target!");
            isTakingTurn = false;
            yield break;
        }
        
        // Move through each tile in the path
        int pathIndex = 0;
        
        // Skip first tile if it's current position
        if (path[0].GridX == currentGridX && path[0].GridY == currentGridY)
        {
            pathIndex = 1;
        }
        
        while (pathIndex < path.Count)
        {
            GridTile targetTile = path[pathIndex];
            Vector3 targetPosition = targetTile.WorldPosition + Vector3.up * heightAboveGround;
            
            // Smoothly move to target
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );
                
                // Rotate towards movement direction
                Vector3 direction = (targetPosition - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }
                
                yield return null;
            }
            
            // Update current grid position
            currentGridX = targetTile.GridX;
            currentGridY = targetTile.GridY;
            
            pathIndex++;
        }
        
        Debug.Log($"Enemy reached position: ({currentGridX}, {currentGridY})");
        if (animator != null) animator.SetBool("isWalking", false);
        isTakingTurn = false;
    }
    
    /// <summary>
    /// Check if the enemy is currently taking its turn
    /// Implementation of IAI interface
    /// </summary>
    public bool IsTakingTurn()
    {
        return isTakingTurn;
    }
    
    /// <summary>
    /// Get the enemy's current grid position
    /// Implementation of IAI interface
    /// </summary>
    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(currentGridX, currentGridY);
    }
    
    /// <summary>
    /// Manually trigger the enemy to take a turn (for testing or manual control)
    /// </summary>
    [ContextMenu("Take Turn")]
    public void ManualTakeTurn()
    {
        TakeTurn();
    }
}
