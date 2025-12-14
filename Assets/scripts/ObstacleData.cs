using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleData", menuName = "Grid System/Obstacle Data")]
public class ObstacleData : ScriptableObject
{
    [Header("Grid Dimensions")]
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    
    [SerializeField] private bool[] obstacleGrid;
    
    public void InitializeGrid(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;
        obstacleGrid = new bool[width * height];
    }
    
    public void SetObstacle(int x, int y, bool hasObstacle)
    {
        if (IsValidPosition(x, y))
        {
            int index = GetIndex(x, y);
            obstacleGrid[index] = hasObstacle;
        }
    }
    
    public bool HasObstacle(int x, int y)
    {
        if (IsValidPosition(x, y))
        {
            int index = GetIndex(x, y);
            return obstacleGrid[index];
        }
        return false;
    }
    
    private int GetIndex(int x, int y)
    {
        return y * gridWidth + x;
    }
    
    /// <summary>
    /// Check if position is within grid bounds
    /// </summary>
    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }
    
    /// <summary>
    /// Clear all obstacles from the grid
    /// </summary>
    public void ClearAllObstacles()
    {
        if (obstacleGrid != null)
        {
            for (int i = 0; i < obstacleGrid.Length; i++)
            {
                obstacleGrid[i] = false;
            }
        }
    }
    
    /// <summary>
    /// Get grid width
    /// </summary>
    public int GetWidth()
    {
        return gridWidth;
    }
    
    /// <summary>
    /// Get grid height
    /// </summary>
    public int GetHeight()
    {
        return gridHeight;
    }
    
    /// <summary>
    /// Ensure the obstacle grid is initialized
    /// </summary>
    private void OnEnable()
    {
        if (obstacleGrid == null || obstacleGrid.Length != gridWidth * gridHeight)
        {
            InitializeGrid(gridWidth, gridHeight);
        }
    }
}
