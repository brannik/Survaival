using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class CustomWindow : EditorWindow
{
    private string[] sceneNames;
    private int selectedSceneIndex = 0;

    private bool hasUnsavedPrefabChanges = false; // Track if the selected prefab has unsaved changes

    [MenuItem("Tools/Open Scene Loader")]
    public static void OpenWindow()
    {
        // Create an instance of the window if it doesn't exist
        CustomWindow window = GetWindow<CustomWindow>("Scene Loader");
        window.Show(); // Show the window
    }

    // Called when the window is created, we populate the scene list
    private void OnEnable()
    {
        // Get the scenes listed in the Build Settings
        sceneNames = GetSceneNamesFromBuildSettings();
    }

    // Example of GUI content inside the window
    private void OnGUI()
    {
        GUILayout.Label("Select a Scene to Load", EditorStyles.boldLabel);

        // Dropdown list with all the scene names from Build Settings
        selectedSceneIndex = EditorGUILayout.Popup("Scenes", selectedSceneIndex, sceneNames);

        // Button to load the selected scene
        if (GUILayout.Button("Load Scene"))
        {
            if (hasUnsavedPrefabChanges)
            {
                EditorGUILayout.HelpBox("Please save the prefab before loading a new scene.", MessageType.Warning);
            }
            else
            {
                LoadScene(sceneNames[selectedSceneIndex]);
            }
        }
    }

    // Method to get the scene names from the Build Settings
    private string[] GetSceneNamesFromBuildSettings()
    {
        string[] names = new string[EditorBuildSettings.scenes.Length];

        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            names[i] = System.IO.Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path);
        }

        return names;
    }

    // Method to load the selected scene
    private void LoadScene(string sceneName)
    {
        // First, check if there are unsaved changes in the current scene
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            string scenePath = GetScenePathByName(sceneName);
            if (!string.IsNullOrEmpty(scenePath))
            {
                Debug.Log($"Loading scene: {scenePath}"); // Debugging log
                EditorSceneManager.OpenScene(scenePath);
            }
            else
            {
                Debug.LogError("Scene path is invalid or not found!");
            }
        }
        else
        {
            Debug.Log("Scene save canceled by user.");
        }
    }

    // Method to get the scene path by its name
    private string GetScenePathByName(string sceneName)
    {
        foreach (var scene in EditorBuildSettings.scenes)
        {
            Debug.Log("Scene Path: " + scene.path); // Debugging log
            if (System.IO.Path.GetFileNameWithoutExtension(scene.path) == sceneName)
            {
                return scene.path; // Return the full path of the scene
            }
        }
        return null; // If not found, return null
    }
}
