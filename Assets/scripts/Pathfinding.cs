using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public int X { get; set; }
    public int Y { get; set; }
    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost => GCost + HCost;
    public PathNode Parent { get; set; }
    
    public PathNode(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class Pathfinding : MonoBehaviour
{
    private GridManager gridManager;
    public static Pathfinding Instance { get; private set; }
    
    void Awake()
    {
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
        gridManager = GridManager.Instance;
        
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found! Pathfinding requires GridManager.");
        }
    }
    
    /// <summary>
    /// Find a path from start tile to target tile using A* algorithm
    /// </summary>
    public List<GridTile> FindPath(int startX, int startY, int targetX, int targetY)
    {
        if (gridManager == null)
        {
            Debug.LogError("GridManager not available for pathfinding!");
            return null;
        }
        
        if (!gridManager.IsValidGridPosition(startX, startY) || 
            !gridManager.IsValidGridPosition(targetX, targetY))
        {
            Debug.LogWarning("Invalid start or target position for pathfinding!");
            return null;
        }
        
        GridTile targetTile = gridManager.GetTile(targetX, targetY);
        if (targetTile == null || !targetTile.IsWalkable())
        {
            Debug.LogWarning("Target tile is not walkable!");
            return null;
        }
        
        if (startX == targetX && startY == targetY)
        {
            return new List<GridTile> { targetTile };
        }
        
        List<PathNode> openList = new List<PathNode>();
        HashSet<string> closedSet = new HashSet<string>();
        
        PathNode startNode = new PathNode(startX, startY);
        startNode.GCost = 0;
        startNode.HCost = CalculateHeuristic(startX, startY, targetX, targetY);
        
        openList.Add(startNode);
        
        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            
            // Check if we reached the target
            if (currentNode.X == targetX && currentNode.Y == targetY)
            {
                return ReconstructPath(currentNode);
            }
            
            // Move current node from open to closed
            openList.Remove(currentNode);
            closedSet.Add(GetNodeKey(currentNode.X, currentNode.Y));
            
            // Check all neighbors (4-directional movement)
            List<PathNode> neighbors = GetNeighbors(currentNode);
            
            foreach (PathNode neighbor in neighbors)
            {
                // Skip if already evaluated
                string neighborKey = GetNodeKey(neighbor.X, neighbor.Y);
                if (closedSet.Contains(neighborKey))
                {
                    continue;
                }
                
                // Check if neighbor tile is walkable
                GridTile neighborTile = gridManager.GetTile(neighbor.X, neighbor.Y);
                if (neighborTile == null || !neighborTile.IsWalkable())
                {
                    closedSet.Add(neighborKey);
                    continue;
                }
                
                // Calculate tentative G cost
                int tentativeGCost = currentNode.GCost + 1; // Cost is 1 for adjacent tiles
                
                // Find existing node in open list
                PathNode existingNode = openList.Find(n => n.X == neighbor.X && n.Y == neighbor.Y);
                
                if (existingNode == null)
                {
                    // New node, add to open list
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = CalculateHeuristic(neighbor.X, neighbor.Y, targetX, targetY);
                    neighbor.Parent = currentNode;
                    openList.Add(neighbor);
                }
                else if (tentativeGCost < existingNode.GCost)
                {
                    // Found a better path to this node
                    existingNode.GCost = tentativeGCost;
                    existingNode.Parent = currentNode;
                }
            }
        }
        
        // No path found
        Debug.LogWarning($"No path found from ({startX}, {startY}) to ({targetX}, {targetY})");
        return null;
    }
    
    /// <summary>
    /// Get the node with the lowest F cost from the open list
    /// </summary>
    private PathNode GetLowestFCostNode(List<PathNode> openList)
    {
        PathNode lowestNode = openList[0];
        for (int i = 1; i < openList.Count; i++)
        {
            if (openList[i].FCost < lowestNode.FCost)
            {
                lowestNode = openList[i];
            }
        }
        return lowestNode;
    }
    
    /// <summary>
    /// Get all valid neighbors of a node (4-directional: up, down, left, right)
    /// </summary>
    private List<PathNode> GetNeighbors(PathNode node)
    {
        List<PathNode> neighbors = new List<PathNode>();
        
        // Four directions: up, down, left, right
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };
        
        for (int i = 0; i < 4; i++)
        {
            int newX = node.X + dx[i];
            int newY = node.Y + dy[i];
            
            if (gridManager.IsValidGridPosition(newX, newY))
            {
                neighbors.Add(new PathNode(newX, newY));
            }
        }
        
        return neighbors;
    }
    
    /// <summary>
    /// Calculate Manhattan distance heuristic for A*
    /// </summary>
    private int CalculateHeuristic(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    }
    
    /// <summary>
    /// Reconstruct the path from end node back to start using parent links
    /// </summary>
    private List<GridTile> ReconstructPath(PathNode endNode)
    {
        List<GridTile> path = new List<GridTile>();
        PathNode currentNode = endNode;
        
        // Trace back from end to start using parent links
        while (currentNode != null)
        {
            GridTile tile = gridManager.GetTile(currentNode.X, currentNode.Y);
            if (tile != null)
            {
                path.Add(tile);
            }
            currentNode = currentNode.Parent;
        }
        
        // Reverse to get path from start to end
        path.Reverse();
        
        return path;
    }
    
    /// <summary>
    /// Generate a unique key for a grid position
    /// </summary>
    private string GetNodeKey(int x, int y)
    {
        return $"{x},{y}";
    }
    
    /// <summary>
    /// Check if a tile at given grid position is walkable
    /// </summary>
    public bool IsTileWalkable(int x, int y)
    {
        if (gridManager == null || !gridManager.IsValidGridPosition(x, y))
        {
            return false;
        }
        
        GridTile tile = gridManager.GetTile(x, y);
        return tile != null && tile.IsWalkable();
    }
}
