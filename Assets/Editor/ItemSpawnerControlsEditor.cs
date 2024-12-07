using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemSpawnerControls))]
public class ItemSpawnerControlsEditor : Editor
{
    // This method will draw the default Inspector UI and add custom buttons
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Draw the default Inspector fields

        ItemSpawnerControls itemSpawner = (ItemSpawnerControls)target;

        // Add a button to the Inspector for creating a random spawn point
        if (GUILayout.Button("Create Random Spawn Point"))
        {
            // Call the method in ItemSpawnerControls to create a spawn point at a random location
            itemSpawner.CreateRandomSpawnPoint();
        }
    }
}
