#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public class CustomConsoleWindow : EditorWindow
{
    private static readonly System.Collections.Generic.List<LogEntry> Logs = new();
    private static bool isRepaintingNeeded = false;

    private static MessageTypeDatabase messageTypeDatabase;
    private string selectedFilterType = "All";  // Default to showing all logs

    private const int buttonSize = 30;  // Fixed size for square buttons
    private const int activeLineHeight = 2;  // Height of the active line at the bottom inside the button
    private const int clearButtonWidth = 30;  // Wider width for "Clear Logs" button
    private const int buttonMargin = 5;  // Margin outside the buttons

    private Vector2 scrollPosition;  // Scroll position for the log area

    [MenuItem("Tools/Custom Console/Console")]
    public static void ShowWindow()
    {
        GetWindow<CustomConsoleWindow>("Custom Console").Show();
    }

    // Method to add a log and refresh the window
    public static void AddCustomLog(string message, string type)
    {
        Logs.Add(new LogEntry { Message = message, Type = type });

        // Set repaint flag to true, will repaint after the game stops or during play mode
        isRepaintingNeeded = true;
    }

    private void OnEnable()
    {
        // Load the MessageTypeDatabase asset
        messageTypeDatabase = AssetDatabase.LoadAssetAtPath<MessageTypeDatabase>("Assets/MessageTypeDatabase.asset");

        // Register to listen to the update cycle
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        // Unsubscribe from the update cycle
        EditorApplication.update -= OnEditorUpdate;
    }

    // This method is called during each editor update cycle (including play mode)
    private void OnEditorUpdate()
    {
        if (isRepaintingNeeded)
        {
            Repaint();
            isRepaintingNeeded = false;
        }
    }

    private void OnGUI()
    {
        // Determine the number of buttons that can fit in the window width
        float windowWidth = position.width - 20; // Account for padding
        int buttonsPerRow = Mathf.FloorToInt(windowWidth / (buttonSize + buttonMargin)); // Adjust for button margin

        // Create a GUIStyle for the buttons with no margins or paddings
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset(0, 0, 0, 0),  // Remove padding inside the button
            margin = new RectOffset(0, 0, 0, 0),   // Remove margin outside the button
            alignment = TextAnchor.MiddleCenter,  // Align the icon inside the button
            fixedHeight = buttonSize,  // Ensure consistent height for the button
            fixedWidth = buttonSize  // Ensure consistent width for the button
        };

        GUILayout.BeginHorizontal(); // Start the first horizontal layout for buttons

        // Clear Logs button (with the specified width)
        if (GUILayout.Button("Clear", buttonStyle, GUILayout.Width(clearButtonWidth), GUILayout.Height(buttonSize)))
        {
            Logs.Clear();
            isRepaintingNeeded = true; // Trigger repaint after clearing
        }

        // "All" button for clearing the filter using the same margin style
        if (GUILayout.Button("All", buttonStyle, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize)))
        {
            selectedFilterType = "All";
        }

        // Display icon buttons for each message type in the database
        if (messageTypeDatabase != null)
        {
            int buttonCount = 0; // Keep track of the number of buttons per row

            foreach (var messageType in messageTypeDatabase.MessageTypes)
            {
                // Count how many logs belong to this message type
                int logCount = Logs.Count(log => log.Type == messageType.Name);

                // Check if the current message type is selected
                bool isActive = selectedFilterType == messageType.Name;

                // Create a GUIContent with the icon and tooltip
                GUIContent buttonContent = new GUIContent(messageType.Icon, messageType.Name);  // Set the tooltip to the message type's name

                // Create the button with the icon (keeping the height fixed but adjusting its contents)
                if (GUILayout.Button(buttonContent, buttonStyle))
                {
                    selectedFilterType = messageType.Name;  // Set the filter to this message type
                }

                // Get the last button's rect and scale the icon properly
                Rect buttonRect = GUILayoutUtility.GetLastRect();  // Get the rect where the button was drawn

                // Make sure the icon is scaled properly within the button without stretching
                GUI.DrawTexture(new Rect(buttonRect.x + (buttonRect.width - buttonSize) / 2, buttonRect.y + (buttonRect.height - buttonSize) / 2, buttonSize, buttonSize), messageType.Icon, ScaleMode.ScaleToFit, true);

                // If this button is active, draw the bottom line inside the button
                if (isActive)
                {
                    GUI.color = Color.yellow;  // Set color for the active indicator line
                    float lineY = buttonRect.yMax - activeLineHeight;  // Position of the line inside the button
                    GUI.DrawTexture(new Rect(buttonRect.x, lineY, buttonRect.width, activeLineHeight), EditorGUIUtility.whiteTexture);  // Draw the line inside the button
                    GUI.color = Color.white;  // Reset color
                }

                // Draw the log count label inside the button (top-right corner)
                // Draw the log count label inside the button (top-right corner)
                GUIStyle countStyle = new GUIStyle()
                {
                    fontSize = 10,
                    fontStyle = FontStyle.Bold,  // Make the text bold
                    alignment = TextAnchor.UpperRight,
                    normal = { textColor = Color.yellow }  // Set the text color to yellow
                };

                // Display the count inside the button (top-right corner)
                if (logCount > 0)
                {
                    Rect countRect = new Rect(buttonRect.x + buttonRect.width - 16, buttonRect.y + 2, 14, 14);

                    // Offset for the outline (drawing it in 4 directions)
                    Vector2[] outlineOffsets = new Vector2[]
                    {
                        new Vector2(-1, -1),  // Top-left
                        new Vector2(1, -1),   // Top-right
                        new Vector2(-1, 1),   // Bottom-left
                        new Vector2(1, 1)     // Bottom-right
                    };

                    // Set the outline color to black
                    GUI.color = Color.black;

                    // Draw the outline by rendering the text 4 times with offset
                    foreach (var offset in outlineOffsets)
                    {
                        // Correct the offset by creating a new Rect for each offset position
                        GUI.Label(new Rect(countRect.x + offset.x, countRect.y + offset.y, countRect.width, countRect.height), logCount.ToString(), countStyle);
                    }

                    // Reset color to yellow for the main text
                    GUI.color = Color.yellow;

                    // Draw the actual count (yellow text)
                    GUI.Label(countRect, logCount.ToString(), countStyle);
                }

                // Move to a new row if we've reached the maximum number of buttons per row
                buttonCount++;
                if (buttonCount >= buttonsPerRow)
                {
                    GUILayout.EndHorizontal();  // End the current horizontal layout
                    GUILayout.BeginHorizontal();  // Begin a new horizontal layout
                    buttonCount = 0;  // Reset the button count for the new row
                }
            }
        }

        GUILayout.EndHorizontal();  // End the second horizontal layout

        GUILayout.Space(10); // Space before logs

        // Create a scrollable area for the logs
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

        // Set lighter background color for the log rows
        Color originalBackgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);  // Light gray background

        // Reverse the order of logs before displaying them
        var reversedLogs = Logs.AsEnumerable().Reverse().ToList();

        // Display filtered logs
        foreach (var log in reversedLogs)
        {
            if (selectedFilterType == "All" || log.Type == selectedFilterType)
            {
                GUILayout.BeginHorizontal();

                // Get the icon based on the message type
                var icon = GetIconForType(log.Type);

                if (icon != null)
                {
                    GUILayout.Label(new GUIContent(icon), GUILayout.Width(20), GUILayout.Height(20));
                }

                // Set text color based on message type
                var messageType = messageTypeDatabase.MessageTypes.FirstOrDefault(mt => mt.Name == log.Type);
                if (messageType != null)
                {
                    GUI.contentColor = messageType.textColor;  // Set the text color from the database
                }

                // Display the log message
                GUILayout.Label($"{log.Message}");

                // Reset content color to default (white)
                GUI.contentColor = Color.white;

                GUILayout.EndHorizontal();
            }
        }

        // Reset the background color to original after the logs
        GUI.backgroundColor = originalBackgroundColor;

        // End the scrollable view
        GUILayout.EndScrollView();
    }

    private Texture2D GetIconForType(string type)
    {
        // If the MessageTypeDatabase is loaded, find the icon for the message type
        if (messageTypeDatabase != null)
        {
            var entry = messageTypeDatabase.MessageTypes.FirstOrDefault(mt => mt.Name == type);
            return entry?.Icon;
        }
        return null;
    }

    private class LogEntry
    {
        public string Message;
        public string Type;
        public Texture2D Icon;
    }
}
#endif
