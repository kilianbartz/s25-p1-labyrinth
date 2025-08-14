using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MazeLoader))]
public class MazeLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MazeLoader loader = (MazeLoader)target;

        GUILayout.Space(10);

        if (GUILayout.Button("🟢 Generate Maze"))
        {
            loader.GenerateMaze();
        }

        if (GUILayout.Button("🗑️ Clear Maze"))
        {
            loader.ClearMaze();
        }
    }
}
