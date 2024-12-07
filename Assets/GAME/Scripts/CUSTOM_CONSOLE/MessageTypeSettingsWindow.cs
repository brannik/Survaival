#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class MessageTypeSettingsWindow : EditorWindow
{
    private MessageTypeDatabase database;
    private bool[] foldouts;

    [MenuItem("Tools/Custom Console/Settings")]
    public static void ShowWindow()
    {
        var window = GetWindow<MessageTypeSettingsWindow>("Custom Console Settings");
        window.Show();
    }

    private void OnEnable()
    {
        database = AssetDatabase.LoadAssetAtPath<MessageTypeDatabase>("Assets/MessageTypeDatabase.asset");
        if (database == null)
        {
            database = CreateInstance<MessageTypeDatabase>();
            AssetDatabase.CreateAsset(database, "Assets/MessageTypeDatabase.asset");
            AssetDatabase.SaveAssets();
        }

        // Initialize foldout states based on the number of message types
        foldouts = new bool[database.MessageTypes.Count];
    }

    private void OnGUI()
    {
        GUILayout.Label("Manage Message Types", EditorStyles.boldLabel);

        // Button to add a new message type
        if (GUILayout.Button("Add New Message Type"))
        {
            database.MessageTypes.Add(new MessageTypeEntry { Name = "New Type" });
            System.Array.Resize(ref foldouts, database.MessageTypes.Count);  // Resize foldout array
        }

        // Loop through all message types and display them in expandable sections
        for (int i = 0; i < database.MessageTypes.Count; i++)
        {
            // Create a foldout for each message type
            foldouts[i] = EditorGUILayout.Foldout(foldouts[i], database.MessageTypes[i].Name, true);
            if (foldouts[i])
            {
                // If the section is expanded, display the properties

                // Editable name
                database.MessageTypes[i].Name = EditorGUILayout.TextField("Type Name", database.MessageTypes[i].Name);

                // Editable icon
                database.MessageTypes[i].Icon = (Texture2D)EditorGUILayout.ObjectField(
                    "Icon",
                    database.MessageTypes[i].Icon,
                    typeof(Texture2D),
                    false
                );

                // Editable color
                database.MessageTypes[i].textColor = EditorGUILayout.ColorField("Text Color", database.MessageTypes[i].textColor);

                // Remove button
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    database.MessageTypes.RemoveAt(i);
                    System.Array.Resize(ref foldouts, database.MessageTypes.Count);  // Resize foldout array
                    break;
                }

                GUILayout.Space(10);
            }
        }

        // Save changes to the database
        if (GUI.changed)
        {
            EditorUtility.SetDirty(database);
        }
    }
}
#endif
