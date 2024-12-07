using UnityEngine;
using UnityEditor;
public static class CustomLogger
{
    private static MessageTypeDatabase database;

    static CustomLogger()
    {
        LoadDatabase();
    }

    private static void LoadDatabase()
    {
#if UNITY_EDITOR
        database = UnityEditor.AssetDatabase.LoadAssetAtPath<MessageTypeDatabase>("Assets/MessageTypeDatabase.asset");
#endif
        if (database == null)
        {
            Debug.LogWarning("MessageTypeDatabase not found. Please create it using the MessageTypeSettingsWindow.");
        }
    }

    public static void Log(string message, string messageTypeName)
    {
        if (database == null)
        {
            Debug.Log($"[Default] {message}");
            return;
        }

        var messageType = database.MessageTypes.Find(type => type.Name == messageTypeName);
        if (messageType != null)
        {
#if UNITY_EDITOR
            CustomConsoleWindow.AddCustomLog(message, messageTypeName);
#endif
        }
        else
        {
            Debug.LogWarning($"Message type '{messageTypeName}' not found. Message: {message}");
        }
    }
}
