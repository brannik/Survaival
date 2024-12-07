using UnityEditor;
using UnityEngine;
using TMPro;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class PrefabManagementWindow : EditorWindow
{
    private ItemSO selectedItem;
    private GameObject prefabObject;  // Prefab object passed from main window
    private string newPrefabName = "NewPrefab";  // Default name for the new prefab
    private int amount = 1;  // Amount for ItemPickup

    private string prefabDirectory = "Assets/GAME/Inventory/PickupItems/Prefabs/";  // Directory to search prefabs in
    private string[] existingPrefabs;  // List of existing prefabs for the specific item

    // Open the prefab management window
    public static void OpenWindow(ItemSO item, GameObject prefabObj, string prefabDirectory)
    {
        PrefabManagementWindow window = GetWindow<PrefabManagementWindow>("Prefab Management");
        window.selectedItem = item;
        window.prefabObject = prefabObj;
        window.prefabDirectory = prefabDirectory;
        window.Show();
    }

    private void OnEnable()
    {
        if (selectedItem != null)
        {
            LoadExistingPrefabs();  // Load existing prefabs when the window is opened
        }
    }

    private void OnGUI()
    {
        if (selectedItem == null || prefabObject == null)
        {
            EditorGUILayout.HelpBox("No item or prefab object selected.", MessageType.Warning);
            return;
        }
        GUIStyle centeredStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,  // Horizontally center the text
            fontStyle = FontStyle.Bold
        };

        // Set a fixed width for the label (optional, but it helps with horizontal centering)
        float labelWidth = EditorGUIUtility.currentViewWidth - 20;  // Adjust the width as needed

        EditorGUILayout.LabelField($"===[{selectedItem.itemName}]===", centeredStyle, GUILayout.Width(labelWidth));
        EditorGUILayout.Space();

        // Field for entering prefab name
        newPrefabName = EditorGUILayout.TextField("Prefab Name", newPrefabName);

        // Field for entering amount
        amount = EditorGUILayout.IntField("Amount", amount);

        // Button to create the new prefab variant
        if (GUILayout.Button("Create Prefab Variant"))
        {
            CreatePrefabVariant();
        }

        if (selectedItem != null)
        {
            LoadExistingPrefabs();  // Load existing prefabs when the window is opened
        }

        // List existing prefabs for the specific item
        EditorGUILayout.LabelField("Existing Prefabs", EditorStyles.boldLabel);

        if (existingPrefabs != null && existingPrefabs.Length > 0)
        {
            foreach (var prefabName in existingPrefabs)
            {
                // Display the prefab name and an input field for the amount
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(prefabName, GUILayout.Width(200));

                // Add a "Drop" label before the amount input field
                EditorGUILayout.LabelField("Drop", GUILayout.Width(40));  // Label for 'amount'

                // Get the prefab path
                string prefabPath = prefabDirectory + prefabName;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                ItemPickup itemPickup = prefab.GetComponent<ItemPickup>();

                // Temporary variable to store the input amount
                int tempAmount = itemPickup?.amount ?? 1;  // Default to 1 if null

                // Create an editable field for the amount (use the temporary variable here)
                tempAmount = EditorGUILayout.IntField(tempAmount, GUILayout.Width(60));

                // Immediately update the prefab if the amount changes
                if (tempAmount != itemPickup.amount)
                {
                    UpdatePrefabAmount(prefab, itemPickup, tempAmount);
                }

                // Delete button to remove the prefab
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    DeletePrefab(prefab, prefabName);  // Delete the selected prefab
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.LabelField("No prefabs found for this item.", EditorStyles.helpBox);
        }
    }

    private void LoadExistingPrefabs()
    {
        // Ensure prefabDirectory is not empty or null
        if (string.IsNullOrEmpty(prefabDirectory))
        {
            Debug.LogError("Prefab directory is not set.");
            return;
        }

        // Make sure the directory exists
        if (!Directory.Exists(prefabDirectory))
        {
            Debug.LogError("Prefab directory does not exist: " + prefabDirectory);
            return;
        }

        // Find all prefabs in the directory
        string[] prefabPaths = Directory.GetFiles(prefabDirectory, "*.prefab", SearchOption.AllDirectories);

        if (prefabPaths.Length == 0)
        {
            Debug.LogWarning("No prefabs found in the directory.");
            return;
        }

        // List to store prefabs with the matching ItemSO
        List<string> filteredPrefabs = new List<string>();

        // Loop through each prefab and check its ItemPickup component
        foreach (var path in prefabPaths)
        {
            // Load the prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            // Check if the prefab has an ItemPickup component
            ItemPickup itemPickup = prefab.GetComponent<ItemPickup>();
            if (itemPickup != null && itemPickup.item == selectedItem)
            {
                // If the ItemSO matches, add the prefab to the list
                filteredPrefabs.Add(Path.GetFileName(path));
            }
        }

        // Convert the list to an array for easier use in the UI
        existingPrefabs = filteredPrefabs.ToArray();


    }

    private void CreatePrefabVariant()
    {
        if (string.IsNullOrEmpty(newPrefabName))
        {
            EditorUtility.DisplayDialog("Error", "Please provide a prefab name.", "OK");
            return;
        }

        // Check if a prefab with the same name already exists
        string path = prefabDirectory + newPrefabName + ".prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            // If the prefab exists, append a random prefix to the name to make it unique
            string randomPrefix = System.Guid.NewGuid().ToString().Substring(0, 8);  // Generate a random 8-character prefix
            newPrefabName = randomPrefix + newPrefabName;
            path = prefabDirectory + newPrefabName + ".prefab";  // Update the path with the new name
        }

        // Find the original prefab in the project (assuming you want to create a variant of the original prefab)
        string originalPrefabPath = AssetDatabase.GetAssetPath(prefabObject);  // Path to the original prefab

        // Check if the original prefab exists
        if (string.IsNullOrEmpty(originalPrefabPath))
        {
            EditorUtility.DisplayDialog("Error", "Original prefab not found.", "OK");
            return;
        }

        // Load the original prefab asset
        GameObject originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(originalPrefabPath);

        // Instantiate the prefab variant
        GameObject prefabVariant = (GameObject)PrefabUtility.InstantiatePrefab(originalPrefab);
        prefabVariant.name = newPrefabName;  // Set the name as per user input

        // Get the ItemPickup script component
        ItemPickup itemPickupScript = prefabVariant.GetComponent<ItemPickup>();
        if (itemPickupScript != null)
        {
            // Set the item and amount in the ItemPickup script
            itemPickupScript.item = selectedItem;
            itemPickupScript.amount = amount;
        }
        else
        {
            Debug.LogError("ItemPickup script not found on prefab.");
            return;
        }

        // Save the prefab variant (it will be linked to the original prefab as a variant)
        PrefabUtility.SaveAsPrefabAssetAndConnect(prefabVariant, path, InteractionMode.UserAction);

        // Optionally destroy the instantiated prefab if it's no longer needed
        DestroyImmediate(prefabVariant);

        AssetDatabase.SaveAssets();
        Debug.Log($"Prefab variant created at {path}");

        // Refresh the list of existing prefabs to include the new one
        LoadExistingPrefabs();
    }

    // Method to update the amount in the prefab
    private void UpdatePrefabAmount(GameObject prefab, ItemPickup itemPickup, int newAmount)
    {
        if (prefab == null || itemPickup == null) return;

        // Update the amount in the ItemPickup component
        itemPickup.amount = newAmount;

        // Save the changes to the prefab immediately
        PrefabUtility.SavePrefabAsset(prefab);

        // Log the change for debugging
        Debug.Log($"Updated amount for prefab '{prefab.name}' to {newAmount}.");
    }

    // Method to delete the prefab variant
    private void DeletePrefab(GameObject prefab, string prefabName)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab to delete not found.");
            return;
        }

        // Delete the prefab asset from the project
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(prefab));

        // Optionally, refresh the list of existing prefabs after deletion
        LoadExistingPrefabs();

        // Log the deletion
        Debug.Log($"Prefab '{prefabName}' deleted.");
    }
}
