using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObstacleManager))]
public class ObstacleManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ObstacleManager obstacleManager = (ObstacleManager)target;
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        if (GUILayout.Button("Refresh Obstacles", GUILayout.Height(30)))
        {
            obstacleManager.RefreshObstacles();
        }
        EditorGUI.EndDisabledGroup();
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to refresh obstacles", MessageType.Info);
        }
        
        if (GUILayout.Button("Clear All Obstacles", GUILayout.Height(30)))
        {
            obstacleManager.ClearObstacles();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Open Grid Obstacle Editor", GUILayout.Height(25)))
        {
            EditorWindow.GetWindow(typeof(GridEditorTool), false, "Grid Obstacle Editor");
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Refresh Obstacles: Reload obstacles from ObstacleData\n" +
            "Clear All Obstacles: Remove all obstacles from scene\n" +
            "Open Grid Obstacle Editor: Edit obstacle placement", 
            MessageType.Info
        );
    }
}
