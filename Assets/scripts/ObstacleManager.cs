using UnityEngine;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObstacleData obstacleData;
    [SerializeField] private GridManager gridManager;
    
    [Header("Obstacle Settings")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private float obstacleHeight = 0.5f;
    [SerializeField] private float obstacleScale = 0.8f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color obstacleColor = Color.red;
    
    private List<GameObject> spawnedObstacles = new List<GameObject>();
    
    void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<GridManager>();
        }
        
        Invoke(nameof(GenerateObstacles), 0.1f);
    }
    
    /// <summary>
    /// Generate obstacles based on the ObstacleData ScriptableObject
    /// </summary>
    public void GenerateObstacles()
    {
        // Clear existing obstacles first
        ClearObstacles();
        
        if (obstacleData == null)
        {
            Debug.LogWarning("ObstacleData is not assigned to ObstacleManager!");
            return;
        }
        
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found! Cannot generate obstacles.");
            return;
        }
        
        // Check if grid has been generated (only happens in Play mode)
        if (gridManager.GetAllTiles() == null)
        {
            Debug.LogWarning("Grid has not been generated yet. Enter Play mode first!");
            return;
        }
        
        // Iterate through the obstacle data and create red spheres
        int width = obstacleData.GetWidth();
        int height = obstacleData.GetHeight();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (obstacleData.HasObstacle(x, y))
                {
                    CreateObstacleAtPosition(x, y);
                }
            }
        }
        
        Debug.Log($"Generated {spawnedObstacles.Count} obstacles");
    }
    
    /// <summary>
    /// Create an obstacle at the specified grid position
    /// </summary>
    /// <param name="gridX">Grid X coordinate</param>
    /// <param name="gridY">Grid Y coordinate</param>
    private void CreateObstacleAtPosition(int gridX, int gridY)
    {
        // Get the tile at this position
        GridTile tile = gridManager.GetTile(gridX, gridY);
        
        if (tile == null)
        {
            Debug.LogWarning($"Tile at ({gridX}, {gridY}) not found!");
            return;
        }
        
        // Calculate obstacle position (above the tile)
        Vector3 obstaclePosition = tile.WorldPosition + Vector3.up * obstacleHeight;
        
        // Create the obstacle GameObject
        GameObject obstacle;
        if (obstaclePrefab != null)
        {
            obstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity, transform);
        }
        else
        {
            // Create a default red sphere
            obstacle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obstacle.transform.position = obstaclePosition;
            obstacle.transform.localScale = Vector3.one * obstacleScale;
            obstacle.transform.parent = transform;
            
            // Set color to red
            Renderer renderer = obstacle.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (renderer.material != null)
                {
                    renderer.material.color = obstacleColor;
                }
                else
                {
                    renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    renderer.material.color = obstacleColor;
                }
            }
            
            // Remove collider so it doesn't interfere with raycasting to tiles
            Collider collider = obstacle.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
        }
        
        obstacle.name = $"Obstacle_{gridX}_{gridY}";
        
        // Mark the tile as having an obstacle
        tile.SetObstacle(obstacle);
        
        // Add to spawned obstacles list
        spawnedObstacles.Add(obstacle);
    }
    
    /// <summary>
    /// Clear all spawned obstacles from the scene
    /// </summary>
    public void ClearObstacles()
    {
        // Destroy all spawned obstacle GameObjects
        foreach (GameObject obstacle in spawnedObstacles)
        {
            if (obstacle != null)
            {
                Destroy(obstacle);
            }
        }
        
        spawnedObstacles.Clear();
        
        // Clear obstacle flags from all tiles
        if (gridManager != null)
        {
            GridTile[,] tiles = gridManager.GetAllTiles();
            if (tiles != null)
            {
                for (int x = 0; x < gridManager.GridWidth; x++)
                {
                    for (int y = 0; y < gridManager.GridHeight; y++)
                    {
                        if (tiles[x, y] != null)
                        {
                            tiles[x, y].SetObstacle(null);
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Refresh obstacles - clear and regenerate from ObstacleData
    /// Useful when obstacles are changed in the editor
    /// </summary>
    [ContextMenu("Refresh Obstacles")]
    public void RefreshObstacles()
    {
        GenerateObstacles();
    }
}
