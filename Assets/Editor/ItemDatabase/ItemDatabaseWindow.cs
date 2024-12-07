using UnityEditor;
using UnityEngine;
using static ENUMS;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ItemDatabaseWindow : EditorWindow
{
    private ItemDatabase itemDatabase;
    private Vector2 scrollPosition;
    private List<int> itemsToRemove = new List<int>();  // List to store items marked for removal
    private GameObject prefabObject;  // Reference to the shared prefab for all items
    private string prefabDirectory = "Assets/GAME/Inventory/PickupItems/Prefabs/";  // Default directory for prefabs
    private string objectDirectory = "Assets/GAME/Inventory/PickupItems/Objects/";  // Default directory for objects
    private string packageDirectory = "Assets/Backup/Packages";  // Default directory for packages

    // New fields for the collapsible region and setup
    private bool isSettingsCollapsed = true;
    private string packageToExtract;


    [MenuItem("Tools/Item Database Manager")]
    public static void OpenWindow()
    {
        ItemDatabaseWindow window = GetWindow<ItemDatabaseWindow>("Item Database Manager");
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Item Database Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Collapsible settings region
        isSettingsCollapsed = EditorGUILayout.Foldout(isSettingsCollapsed, "Settings");
        if (isSettingsCollapsed)
        {
            EditorGUILayout.BeginVertical("box");

            // Allow selection of the ItemDatabase
            itemDatabase = (ItemDatabase)EditorGUILayout.ObjectField("Item Database", itemDatabase, typeof(ItemDatabase), true);

            if (itemDatabase == null)
            {
                EditorGUILayout.HelpBox("Please assign an ItemDatabase to manage.", MessageType.Warning);
            }

            // Select the prefab used for all items
            prefabObject = (GameObject)EditorGUILayout.ObjectField("Item Prefab", prefabObject, typeof(GameObject), false);

            if (prefabObject == null)
            {
                EditorGUILayout.HelpBox("Please assign a prefab for all items.", MessageType.Warning);
            }

            EditorGUILayout.Space();

            // Directory selection for prefabs
            EditorGUILayout.LabelField("Prefab Directory");
            prefabDirectory = EditorGUILayout.TextField(prefabDirectory);
            if (GUILayout.Button("Select Prefab Directory"))
            {
                // Open a folder picker to select the prefab directory
                string selectedPath = EditorUtility.OpenFolderPanel("Select Prefab Directory", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    prefabDirectory = "Assets" + selectedPath.Substring(Application.dataPath.Length); // Ensure it's a relative path
                }
            }

            // Directory selection for objects
            EditorGUILayout.LabelField("Object Directory");
            objectDirectory = EditorGUILayout.TextField(objectDirectory);
            if (GUILayout.Button("Select Object Directory"))
            {
                // Open a folder picker to select the object directory
                string selectedPath = EditorUtility.OpenFolderPanel("Select Object Directory", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    objectDirectory = "Assets" + selectedPath.Substring(Application.dataPath.Length); // Ensure it's a relative path
                }
            }

            // Package related buttons
            EditorGUILayout.Space();
            if (GUILayout.Button("Create Package of Scripts and Objects"))
            {
                CreatePackage();
            }

            if (GUILayout.Button("Setup (Extract Package)"))
            {
                ExtractPackage();
            }

            EditorGUILayout.EndVertical(); // End the settings region
        }

        EditorGUILayout.Space();

        // Button to open the new item creation window
        if (GUILayout.Button("Create New Item"))
        {
            OpenCreateNewItemWindow();
        }

        EditorGUILayout.Space();

        // Start scrollable list of items
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < itemDatabase.ItemsDB.Count; i++)
        {
            ItemSO item = itemDatabase.ItemsDB[i];

            EditorGUILayout.BeginHorizontal(); // Begin item row

            if (item == null)
            {
                EditorGUILayout.LabelField($"Item {i}: [NULL]");

                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    // Defer item removal by adding it to the list
                    itemsToRemove.Add(i);
                    break;  // Exit the loop to avoid modifying the list during iteration
                }
            }
            else
            {
                EditorGUILayout.LabelField($"[{item.itemId}] - {item.itemName}");

                // Edit button for item
                if (GUILayout.Button("Edit", GUILayout.Width(80)))
                {
                    OpenEditWindow(item);
                }

                // Prefab button for item with count of attached prefabs
                int prefabCount = GetPrefabCountForItem(item);  // Get count of prefabs attached to the item
                if (GUILayout.Button($"Prefab ({prefabCount})", GUILayout.Width(80)))
                {
                    OpenPrefabWindow(item);
                }

                // Remove button for item
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    itemsToRemove.Add(i);  // Mark item for removal
                    break;  // Exit the loop immediately
                }
            }

            EditorGUILayout.EndHorizontal();  // End item row
        }

        EditorGUILayout.EndScrollView();  // End scroll view

        // Ensure removal happens after the GUI has finished rendering
        if (itemsToRemove.Count > 0)
        {
            EditorApplication.delayCall += () =>
            {
                RemoveItemsInBatch();
            };
        }
    }



    private void RemoveItemsInBatch()
    {
        if (itemsToRemove.Count == 0) return;

        // Sort indices in descending order to prevent shifting during removal
        itemsToRemove.Sort((a, b) => b.CompareTo(a));

        foreach (int index in itemsToRemove)
        {
            RemoveItemAt(index);  // Remove item after layout
        }

        itemsToRemove.Clear();  // Clear the list after processing
        Repaint();  // Refresh the window to reflect changes
    }

    private void RemoveItemAt(int index)
    {
        if (itemDatabase == null || index < 0 || index >= itemDatabase.ItemsDB.Count) return;

        ItemSO itemToRemove = itemDatabase.ItemsDB[index];

        // Confirm deletion from the project if it's an asset
        if (itemToRemove != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(itemToRemove);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath); // Delete the item asset from the project
            }
        }

        // Remove the item from the list
        itemDatabase.ItemsDB.RemoveAt(index);

        // Mark the database as dirty so Unity knows it has changed
        EditorUtility.SetDirty(itemDatabase);

        // Optionally, re-save the database assets if necessary
        AssetDatabase.SaveAssets();

        Debug.Log($"Item {index} removed from the database.");
    }

    private void OpenCreateNewItemWindow()
    {
        NewItemCreationWindow.OpenWindow(itemDatabase, objectDirectory);  // Open the new item creation window
    }

    private void OpenEditWindow(ItemSO item)
    {
        // Open edit window with delay to avoid layout issues
        EditorApplication.delayCall += () =>
        {
            ItemEditWindow.OpenWindow(item, itemDatabase);  // Open the edit window in a delayed call
        };
    }

    private void OpenPrefabWindow(ItemSO item)
    {
        if (prefabObject != null)
        {
            // Open the prefab management window for this specific item and prefab, passing the prefab directory
            PrefabManagementWindow.OpenWindow(item, prefabObject, prefabDirectory);
        }
        else
        {
            EditorUtility.DisplayDialog("Prefab Missing", "Please assign a prefab before managing prefabs.", "OK");
        }
    }

    // Method to get the count of prefabs attached to the specified ItemSO
    private int GetPrefabCountForItem(ItemSO item)
    {
        int prefabCount = 0;

        // Ensure the prefab directory is set correctly
        if (string.IsNullOrEmpty(prefabDirectory) || !System.IO.Directory.Exists(prefabDirectory))
        {
            Debug.LogWarning("Invalid prefab directory.");
            return prefabCount;
        }

        string[] prefabPaths = System.IO.Directory.GetFiles(prefabDirectory, "*.prefab", System.IO.SearchOption.AllDirectories);

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                ItemPickup itemPickup = prefab.GetComponent<ItemPickup>();
                if (itemPickup != null && itemPickup.item == item)
                {
                    prefabCount++;
                }
            }
        }

        return prefabCount;
    }

    private void CreatePackage()
    {
        // Ensure the package directory exists
        if (!Directory.Exists(packageDirectory))
        {
            Directory.CreateDirectory(packageDirectory);
        }

        // Collect the assets to be included in the package
        string[] assetPaths = CollectAssetsForPackage();

        if (assetPaths.Length > 0)
        {
            // Define the package name (optional: include timestamp or version)
            string packageName = "ItemDatabasePackage_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unitypackage";
            string packagePath = Path.Combine(packageDirectory, packageName);

            // Create the Unity package
            AssetDatabase.ExportPackage(assetPaths, packagePath, ExportPackageOptions.Interactive | ExportPackageOptions.Recurse);

            Debug.Log($"Package created: {packagePath}");
        }
        else
        {
            Debug.LogWarning("No assets to include in the package.");
        }
    }

    private string[] CollectAssetsForPackage()
    {
        // Collect scripts and prefabs in the selected directories
        var scriptPaths = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories)
                                   .Where(path => path.StartsWith("Assets/GAME") && !path.EndsWith(".Editor.cs"))
                                   .ToArray();

        var prefabPaths = Directory.GetFiles(prefabDirectory, "*.prefab", SearchOption.AllDirectories)
                                    .Where(path => path.StartsWith("Assets/GAME"))
                                    .ToArray();

        var objectPaths = Directory.GetFiles(objectDirectory, "*.prefab", SearchOption.AllDirectories)
                                    .Where(path => path.StartsWith("Assets/GAME"))
                                    .ToArray();

        // Combine all the asset paths
        var allAssetPaths = scriptPaths.Concat(prefabPaths).Concat(objectPaths).ToArray();

        return allAssetPaths;
    }
    private void ExtractPackage()
    {
        // Allow user to select the package file to extract
        packageToExtract = EditorUtility.OpenFilePanel("Select Unity Package to Extract", "Assets/Backup/Packages", "unitypackage");

        if (string.IsNullOrEmpty(packageToExtract))
        {
            Debug.LogWarning("No package selected.");
            return;
        }

        // Extract the selected package
        AssetDatabase.ImportPackage(packageToExtract, true);

        Debug.Log($"Package extracted: {packageToExtract}");
    }

}
