using UnityEngine;

public class GridTile : MonoBehaviour
{
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public bool HasObstacle { get; set; }
    public bool IsOccupiedByEntity { get; private set; }
    
    private GameObject obstacleObject;
    private GameObject occupyingEntity;
    
    public void Initialize(int x, int y, Vector3 worldPos)
    {
        GridX = x;
        GridY = y;
        WorldPosition = worldPos;
        HasObstacle = false;
        IsOccupiedByEntity = false;
        occupyingEntity = null;
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
        return !HasObstacle && !IsOccupiedByEntity;
    }
    
    public void SetOccupyingEntity(GameObject entity)
    {
        occupyingEntity = entity;
        IsOccupiedByEntity = entity != null;
    }
    
    public GameObject GetOccupyingEntity()
    {
        return occupyingEntity;
    }
}
