using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class BackupMyEditorScripts : EditorWindow
{
    private string editorFolderPath = "Assets/Editor"; // Default folder path for Editor scripts
    private List<string> editorScriptPaths = new List<string>(); // List to hold all script paths in the Editor folder
    private List<bool> selectedEditorScripts = new List<bool>(); // List to track which editor scripts are selected for backup

    private string backupDirectory = "Assets/Backup"; // Backup directory for the package
    private string editorPackageName = "EditorScriptsBackup";

    [MenuItem("Tools/Backup My Scripts")]
    public static void OpenWindow()
    {
        BackupMyEditorScripts window = GetWindow<BackupMyEditorScripts>("Backup My Editor Scripts");
        window.Show();
    }

    private void OnEnable()
    {
        // Find all scripts in the "Editor" folder
        UpdateScriptList();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Backup My Editor Scripts", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Editor scripts backup section
        EditorGUILayout.LabelField("Editor Scripts Backup");
        EditorGUILayout.LabelField("Editor Folder");
        editorFolderPath = EditorGUILayout.TextField(editorFolderPath);

        if (GUILayout.Button("Select Editor Folder"))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Editor Folder", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                editorFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length); // Ensure it's a relative path
                UpdateScriptList(); // Update the list of scripts based on the selected folder
            }
        }

        // Display the list of Editor scripts with checkboxes
        for (int i = 0; i < editorScriptPaths.Count; i++)
        {
            selectedEditorScripts[i] = EditorGUILayout.ToggleLeft(Path.GetFileName(editorScriptPaths[i]), selectedEditorScripts[i]);
        }

        EditorGUILayout.Space();

        // Button to create package for Editor scripts
        if (GUILayout.Button("Create Backup of Editor Scripts"))
        {
            CreateEditorBackupPackage();
        }

        EditorGUILayout.Space();

        // Button to install from a package
        if (GUILayout.Button("Install from Package"))
        {
            InstallFromPackage();
        }
    }

    // Method to update the list of script paths from the selected Editor folder
    private void UpdateScriptList()
    {
        if (Directory.Exists(editorFolderPath))
        {
            editorScriptPaths = Directory.GetFiles(editorFolderPath, "*.cs", SearchOption.AllDirectories).ToList();
            selectedEditorScripts = new List<bool>(new bool[editorScriptPaths.Count]); // Reset selection state
        }
        else
        {
            Debug.LogWarning("The specified Editor folder does not exist.");
            editorScriptPaths.Clear();
            selectedEditorScripts.Clear();
        }
    }

    // Method to create a Unity package for Editor scripts
    private void CreateEditorBackupPackage()
    {
        if (editorScriptPaths.Count == 0)
        {
            Debug.LogWarning("No Editor scripts found to back up.");
            return;
        }

        // Ensure the backup directory exists
        if (!Directory.Exists(backupDirectory))
        {
            Directory.CreateDirectory(backupDirectory);
        }

        // Filter the selected editor scripts
        List<string> selectedPaths = new List<string>();
        for (int i = 0; i < editorScriptPaths.Count; i++)
        {
            if (selectedEditorScripts[i])
            {
                selectedPaths.Add(editorScriptPaths[i]);
            }
        }

        if (selectedPaths.Count == 0)
        {
            Debug.LogWarning("No Editor scripts selected for backup.");
            return;
        }

        // Define the package name
        string packageFileName = $"{editorPackageName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.unitypackage";
        string packagePath = Path.Combine(backupDirectory, packageFileName);

        // Create the Unity package
        AssetDatabase.ExportPackage(selectedPaths.ToArray(), packagePath, ExportPackageOptions.Interactive | ExportPackageOptions.Recurse);

        Debug.Log($"Editor Backup package created: {packagePath}");
    }

    // Method to install selected package
    private void InstallFromPackage()
    {
        string[] packagePaths = Directory.GetFiles(backupDirectory, "*.unitypackage");

        if (packagePaths.Length == 0)
        {
            Debug.LogWarning("No package found to install from.");
            return;
        }

        // Open the Unity package file dialog to choose the package to install
        string packageToInstall = EditorUtility.OpenFilePanel("Select Backup Package", backupDirectory, "unitypackage");

        if (string.IsNullOrEmpty(packageToInstall))
        {
            return; // User canceled
        }

        // Import the selected package into the project
        AssetDatabase.ImportPackage(packageToInstall, true);

        Debug.Log($"Package installed from: {packageToInstall}");
    }
}
