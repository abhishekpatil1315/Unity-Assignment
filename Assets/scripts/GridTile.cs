using UnityEngine;

public class GridTile : MonoBehaviour
{
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public bool HasObstacle { get; set; }
    
    private GameObject obstacleObject;
    
    public void Initialize(int x, int y, Vector3 worldPos)
    {
        GridX = x;
        GridY = y;
        WorldPosition = worldPos;
        HasObstacle = false;
    }
    
    public void SetObstacle(GameObject obstacle)
    {
        obstacleObject = obstacle;
        HasObstacle = obstacle != null;
    }
    
    public string GetTileInfo()
    {
        return $"Grid Position: ({GridX}, {GridY})\nWorld Position: {WorldPosition}\nObstacle: {(HasObstacle ? "Yes" : "No")}";
    }
    
    public bool IsWalkable()
    {
        return !HasObstacle;
    }
}
