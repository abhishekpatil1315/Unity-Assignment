using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float heightAboveGround = 0.5f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color playerColor = Color.blue;
    
    private int currentGridX;
    private int currentGridY;
    
    private GridManager gridManager;
    private Pathfinding pathfinding;
    private Animator animator;
    
    private bool isMoving = false;
    private List<GridTile> currentPath;
    private int currentPathIndex = 0;
    
    public bool IsMoving => isMoving;
    public Vector2Int CurrentGridPosition => new Vector2Int(currentGridX, currentGridY);
    
    void Start()
    {
        gridManager = GridManager.Instance;
        pathfinding = Pathfinding.Instance;
        
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found! PlayerController requires GridManager.");
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
        InitializePosition(5, 0);
    }
    
    void Update()
    {
        if (!isMoving)
        {
            HandleMouseInput();
        }
    }
    
    private void SetupVisuals()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (renderer.material != null)
            {
                renderer.material.color = playerColor;
            }
            else
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                renderer.material.color = playerColor;
            }
        }
    }
    
    /// <summary>
    /// Initialize player position at specific grid coordinates
    /// </summary>
    /// <param name="gridX">Grid X coordinate</param>
    /// <param name="gridY">Grid Y coordinate</param>
    public void InitializePosition(int gridX, int gridY)
    {
        if (gridManager == null || !gridManager.IsValidGridPosition(gridX, gridY))
        {
            Debug.LogWarning("Invalid initialization position!");
            return;
        }
        
        // Clear previous tile occupation
        GridTile previousTile = gridManager.GetTile(currentGridX, currentGridY);
        if (previousTile != null)
        {
            previousTile.SetOccupyingEntity(null);
        }
        
        currentGridX = gridX;
        currentGridY = gridY;
        
        GridTile tile = gridManager.GetTile(gridX, gridY);
        if (tile != null)
        {
            Vector3 targetPosition = tile.WorldPosition + Vector3.up * heightAboveGround;
            transform.position = targetPosition;
            tile.SetOccupyingEntity(gameObject);
        }
    }
    
    /// <summary>
    /// Handle mouse input for tile selection
    /// </summary>
    private void HandleMouseInput()
    {
        // Check for left mouse click
        if (GetMouseButtonDown())
        {
            Vector3 mousePosition = GetMousePosition();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                // Check if we clicked on a tile
                GridTile targetTile = hit.collider.GetComponent<GridTile>();
                
                if (targetTile != null && targetTile.IsWalkable())
                {
                    // Request movement to this tile
                    MoveToTile(targetTile.GridX, targetTile.GridY);
                }
            }
        }
    }
    
    /// <summary>
    /// Move the player to the specified grid position
    /// </summary>
    /// <param name="targetX">Target grid X coordinate</param>
    /// <param name="targetY">Target grid Y coordinate</param>
    public void MoveToTile(int targetX, int targetY)
    {
        if (isMoving)
        {
            Debug.Log("Player is already moving!");
            return;
        }
        
        if (pathfinding == null)
        {
            Debug.LogError("Pathfinding not available!");
            return;
        }
        
        // Don't move if already at target
        if (currentGridX == targetX && currentGridY == targetY)
        {
            Debug.Log("Already at target position!");
            return;
        }
        
        // Find path to target
        List<GridTile> path = pathfinding.FindPath(currentGridX, currentGridY, targetX, targetY);
        
        if (path != null && path.Count > 0)
        {
            // Start movement coroutine
            currentPath = path;
            currentPathIndex = 0;
            StartCoroutine(MoveAlongPath());
        }
        else
        {
            Debug.Log("No valid path to target!");
        }
    }
    
    /// <summary>
    /// Coroutine to smoothly move the player along the calculated path
    /// </summary>
    private IEnumerator MoveAlongPath()
    {
        isMoving = true;
        if (animator != null) animator.SetBool("isWalking", true);
        
        // Skip the first tile (current position)
        if (currentPath[0].GridX == currentGridX && currentPath[0].GridY == currentGridY)
        {
            currentPathIndex = 1;
        }
        
        // Move through each tile in the path
        while (currentPathIndex < currentPath.Count)
        {
            GridTile targetTile = currentPath[currentPathIndex];
            Vector3 targetPosition = targetTile.WorldPosition + Vector3.up * heightAboveGround;
            
            // Smoothly move to the target position
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                // Move towards target
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    targetPosition, 
                    moveSpeed * Time.deltaTime
                );
                
                // Rotate towards target
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
            GridTile oldTile = gridManager.GetTile(currentGridX, currentGridY);
            if (oldTile != null)
            {
                oldTile.SetOccupyingEntity(null);
            }
            
            currentGridX = targetTile.GridX;
            currentGridY = targetTile.GridY;
            targetTile.SetOccupyingEntity(gameObject);
            
            // Move to next tile in path
            currentPathIndex++;
        }
        
        // Movement complete
        isMoving = false;
        if (animator != null) animator.SetBool("isWalking", false);
        currentPath = null;
        currentPathIndex = 0;
        
        Debug.Log($"Player reached destination: ({currentGridX}, {currentGridY})");
    }
    
    /// <summary>
    /// Get the player's current grid position
    /// </summary>
    public void GetCurrentGridPosition(out int x, out int y)
    {
        x = currentGridX;
        y = currentGridY;
    }
    
    /// <summary>
    /// Stop current movement (if needed)
    /// </summary>
    public void StopMovement()
    {
        if (isMoving)
        {
            StopAllCoroutines();
            isMoving = false;
            currentPath = null;
            currentPathIndex = 0;
        }
    }
    
    /// <summary>
    /// Get mouse position supporting both old and new Input System
    /// </summary>
    private Vector3 GetMousePosition()
    {
        #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
        #endif
        return Input.mousePosition;
    }
    
    /// <summary>
    /// Get mouse button down supporting both old and new Input System
    /// </summary>
    private bool GetMouseButtonDown()
    {
        #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }
        #endif
        return Input.GetMouseButtonDown(0);
    }
}
