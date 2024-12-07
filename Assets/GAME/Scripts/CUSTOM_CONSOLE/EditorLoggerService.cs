#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class EditorLoggerService : ICustomLoggerService
{
    public void Log(string message, string type = "Info")
    {
        // Load the message type database
        MessageTypeDatabase database = AssetDatabase.LoadAssetAtPath<MessageTypeDatabase>("Assets/MessageTypeDatabase.asset");

        if (database == null)
        {
            Debug.LogWarning("MessageTypeDatabase not found.");
            return;
        }

        // Find the message type in the database
        var messageType = database.MessageTypes.Find(typeEntry => typeEntry.Name == type);

        // If the message type is found, use its icon
        Texture2D icon = messageType?.Icon;

        if (messageType != null)
        {
            // Add the log to the custom console window
            CustomConsoleWindow.AddCustomLog(message, type);
        }
        else
        {
            // Log a warning if the message type is not found
            Debug.LogWarning($"Message type '{type}' not found. Message: {message}");
        }
    }
}
#endif
