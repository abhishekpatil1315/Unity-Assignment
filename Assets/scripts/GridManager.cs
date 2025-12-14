using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float tileSpacing = 0.1f;
    
    [Header("Prefab References")]
    [SerializeField] private GameObject tilePrefab;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI tileInfoText;
    
    [Header("Visual Settings")]
    [SerializeField] private Material defaultTileMaterial;
    [SerializeField] private Material highlightedTileMaterial;
    
    private GridTile[,] gridTiles;
    private GridTile currentHighlightedTile;
    private Renderer currentHighlightedRenderer;
    
    public static GridManager Instance { get; private set; }
    
    // Public property to access grid dimensions
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float TileSize => tileSize;
    public float TileSpacing => tileSpacing;
    
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
        GenerateGrid();
    }
    
    void Update()
    {
        HandleMouseRaycast();
    }
    
    /// <summary>
    /// Generate the 10x10 grid of cube tiles
    /// </summary>
    private void GenerateGrid()
    {
        gridTiles = new GridTile[gridWidth, gridHeight];
        
        float totalWidth = (gridWidth * tileSize) + ((gridWidth - 1) * tileSpacing);
        float totalHeight = (gridHeight * tileSize) + ((gridHeight - 1) * tileSpacing);
        Vector3 startPosition = transform.position - new Vector3(totalWidth / 2f, 0, totalHeight / 2f);
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                float xPos = startPosition.x + (x * (tileSize + tileSpacing)) + (tileSize / 2f);
                float zPos = startPosition.z + (y * (tileSize + tileSpacing)) + (tileSize / 2f);
                Vector3 tilePosition = new Vector3(xPos, 0, zPos);
                
                GameObject tileObject;
                if (tilePrefab != null)
                {
                    tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity, transform);
                }
                else
                {
                    // Create a default cube if no prefab is assigned
                    tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tileObject.transform.position = tilePosition;
                    tileObject.transform.localScale = new Vector3(tileSize, 0.1f, tileSize);
                    tileObject.transform.parent = transform;
                }
                
                tileObject.name = $"Tile_{x}_{y}";
                
                // Add and initialize GridTile component
                GridTile tileScript = tileObject.AddComponent<GridTile>();
                tileScript.Initialize(x, y, tilePosition);
                
                // Apply default material if available
                if (defaultTileMaterial != null)
                {
                    Renderer renderer = tileObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = defaultTileMaterial;
                    }
                }
                
                // Store in grid array
                gridTiles[x, y] = tileScript;
            }
        }
        
        Debug.Log($"Grid generated: {gridWidth}x{gridHeight} tiles");
    }
    
    /// <summary>
    /// Cast a ray from mouse position to detect which tile is being hovered
    /// </summary>
    private void HandleMouseRaycast()
    {
        Vector3 mousePosition = GetMousePosition();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        // Perform raycast
        if (Physics.Raycast(ray, out hit))
        {
            // Check if we hit a tile
            GridTile tile = hit.collider.GetComponent<GridTile>();
            
            if (tile != null)
            {
                // Update highlighted tile
                if (currentHighlightedTile != tile)
                {
                    // Reset previous tile's material
                    if (currentHighlightedTile != null && currentHighlightedRenderer != null)
                    {
                        currentHighlightedRenderer.material = defaultTileMaterial;
                    }
                    
                    // Highlight new tile
                    currentHighlightedTile = tile;
                    currentHighlightedRenderer = hit.collider.GetComponent<Renderer>();
                    if (highlightedTileMaterial != null && currentHighlightedRenderer != null)
                    {
                        currentHighlightedRenderer.material = highlightedTileMaterial;
                    }
                    
                    // Update UI with tile information
                    UpdateTileInfoUI(tile);
                }
            }
        }
        else
        {
            // No tile hit, clear highlighting
            ClearHighlight();
        }
    }
    
    /// <summary>
    /// Update the UI text with the hovered tile's information
    /// </summary>
    /// <param name="tile">The tile to display info for</param>
    private void UpdateTileInfoUI(GridTile tile)
    {
        if (tileInfoText != null)
        {
            tileInfoText.text = tile.GetTileInfo();
        }
    }
    
    /// <summary>
    /// Clear the current tile highlighting
    /// </summary>
    private void ClearHighlight()
    {
        if (currentHighlightedTile != null && currentHighlightedRenderer != null)
        {
            currentHighlightedRenderer.material = defaultTileMaterial;
            currentHighlightedTile = null;
            currentHighlightedRenderer = null;
            
            if (tileInfoText != null)
            {
                tileInfoText.text = "Hover over a tile";
            }
        }
    }
    
    /// <summary>
    /// Get a specific tile by grid coordinates
    /// </summary>
    public GridTile GetTile(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return gridTiles[x, y];
        }
        return null;
    }
    
    /// <summary>
    /// Check if grid coordinates are within bounds
    /// </summary>
    public bool IsValidGridPosition(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }
    
    /// <summary>
    /// Get all tiles in the grid
    /// </summary>
    public GridTile[,] GetAllTiles()
    {
        return gridTiles;
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
}
