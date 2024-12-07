using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkSpawner))]
public class NetworkSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Draw the default inspector

        NetworkSpawner spawner = (NetworkSpawner)target;

        // Create a button to generate a new spawn point
        if (GUILayout.Button("Generate Spawn Point"))
        {
            spawner.GenerateSpawnPoint();
        }
    }
}