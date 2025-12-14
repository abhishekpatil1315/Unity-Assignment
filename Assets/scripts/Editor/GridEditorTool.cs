using UnityEngine;
using UnityEditor;

public class GridEditorTool : EditorWindow
{
    private ObstacleData obstacleData;
    private Vector2 scrollPosition;
    
    private const int GRID_WIDTH = 10;
    private const int GRID_HEIGHT = 10;
    private const float BUTTON_SIZE = 40f;
    
    [MenuItem("Tools/Grid Obstacle Editor")]
    public static void ShowWindow()
    {
        GridEditorTool window = GetWindow<GridEditorTool>("Grid Obstacle Editor");
        window.minSize = new Vector2(500, 600);
        window.Show();
    }
    
    private void OnEnable()
    {
        LoadObstacleData();
    }
    
    private void OnGUI()
    {
        GUILayout.Space(10);
        
        EditorGUILayout.LabelField("Grid Obstacle Editor", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Obstacle Data:", GUILayout.Width(100));
        ObstacleData newData = (ObstacleData)EditorGUILayout.ObjectField(obstacleData, typeof(ObstacleData), false);
        
        if (newData != obstacleData)
        {
            obstacleData = newData;
            if (obstacleData != null)
            {
                // Ensure grid is initialized with correct dimensions
                obstacleData.InitializeGrid(GRID_WIDTH, GRID_HEIGHT);
                EditorUtility.SetDirty(obstacleData);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // Create new ObstacleData button
        if (obstacleData == null)
        {
            if (GUILayout.Button("Create New Obstacle Data", GUILayout.Height(30)))
            {
                CreateNewObstacleData();
            }
            
            EditorGUILayout.HelpBox("Please assign or create an ObstacleData ScriptableObject to edit obstacles.", MessageType.Info);
            return;
        }
        
        // Instructions
        EditorGUILayout.HelpBox("Toggle buttons to place/remove obstacles on the grid.\nGreen = Walkable, Red = Obstacle", MessageType.Info);
        GUILayout.Space(10);
        
        // Clear all button
        if (GUILayout.Button("Clear All Obstacles", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Clear All Obstacles", 
                "Are you sure you want to clear all obstacles?", "Yes", "No"))
            {
                obstacleData.ClearAllObstacles();
                EditorUtility.SetDirty(obstacleData);
                AssetDatabase.SaveAssets();
            }
        }
        
        GUILayout.Space(10);
        
        // Draw the grid of buttons
        DrawObstacleGrid();
        
        GUILayout.Space(10);
        
        // Save button (optional - changes are saved automatically)
        if (GUILayout.Button("Save Changes", GUILayout.Height(25)))
        {
            EditorUtility.SetDirty(obstacleData);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Saved", "Obstacle data saved successfully!", "OK");
        }
    }
    
    /// <summary>
    /// Draw the 10x10 grid of toggleable buttons
    /// </summary>
    private void DrawObstacleGrid()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Draw grid from top to bottom (y axis inverted for visual representation)
        for (int y = GRID_HEIGHT - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Y-axis label
            GUILayout.Label(y.ToString(), GUILayout.Width(20));
            
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                // Get current obstacle state
                bool hasObstacle = obstacleData.HasObstacle(x, y);
                
                // Set button color based on state
                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = hasObstacle ? new Color(1f, 0.3f, 0.3f) : new Color(0.3f, 1f, 0.3f);
                
                // Draw toggle button
                if (GUILayout.Button($"{x},{y}", GUILayout.Width(BUTTON_SIZE), GUILayout.Height(BUTTON_SIZE)))
                {
                    // Toggle obstacle state
                    obstacleData.SetObstacle(x, y, !hasObstacle);
                    EditorUtility.SetDirty(obstacleData);
                }
                
                // Restore original color
                GUI.backgroundColor = originalColor;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        // X-axis labels
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(20)); // Offset for y-axis label
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            GUILayout.Label(x.ToString(), GUILayout.Width(BUTTON_SIZE));
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndScrollView();
    }
    
    /// <summary>
    /// Try to load existing ObstacleData from the project
    /// </summary>
    private void LoadObstacleData()
    {
        if (obstacleData == null)
        {
            // Try to find an existing ObstacleData in the project
            string[] guids = AssetDatabase.FindAssets("t:ObstacleData");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                obstacleData = AssetDatabase.LoadAssetAtPath<ObstacleData>(path);
            }
        }
    }
    
    /// <summary>
    /// Create a new ObstacleData ScriptableObject
    /// </summary>
    private void CreateNewObstacleData()
    {
        // Create the ScriptableObject
        obstacleData = ScriptableObject.CreateInstance<ObstacleData>();
        obstacleData.InitializeGrid(GRID_WIDTH, GRID_HEIGHT);
        
        // Save it to the project
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Obstacle Data",
            "ObstacleData",
            "asset",
            "Please enter a file name to save the obstacle data");
        
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(obstacleData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "Obstacle Data created successfully!", "OK");
        }
    }
}
