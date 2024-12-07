using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GatherableSpawnerControls))]
public class GatherableSpawnerControlsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GatherableSpawnerControls spawnerControls = (GatherableSpawnerControls)target;

        // Button to create a random spawn point
        if (GUILayout.Button("Create Random Spawn Point"))
        {
            spawnerControls.CreateRandomSpawnPoint();
        }

        // Button to create multiple random spawn points
        if (GUILayout.Button("Create Multiple Spawn Points"))
        {
            spawnerControls.CreateMultipleSpawnPoints();
        }

        // Button to remove all spawn points
        if (GUILayout.Button("Remove All Spawn Points"))
        {
            spawnerControls.RemoveAllSpawnPoints();
        }
    }
}
