using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SpawnPoint : MonoBehaviour
{
    [Header("Visualization Settings")]
    [SerializeField] private Color gizmoColor = Color.green; // Color of the arrow
    [SerializeField] private float arrowLength = 1.0f; // Length of the arrow
    [SerializeField] private float arrowHeadLength = 0.3f; // Length of the arrow head
    [SerializeField] private float arrowHeadAngle = 20.0f; // Angle of the arrowhead

    [Header("Label Settings")]
    [SerializeField] private Color labelTextColor = Color.white; // Color of the text
    [SerializeField] private Color labelBackgroundColor = Color.black; // Color of the background

    // Draw an arrow pointing downward with a label for the spawn point
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        // Position of the arrow start point (spawn point)
        Vector3 start = transform.position;

        // Direction the arrow points (downwards)
        Vector3 direction = Vector3.down * arrowLength;

        // End point of the arrow shaft
        Vector3 end = start + direction;

        // Draw the arrow shaft
        Gizmos.DrawLine(start, end);

        // Draw the arrowhead
        DrawArrowhead(end, direction);

        // Add a label at the spawn point position with background
        DrawLabelWithBackground(start + Vector3.up * 0.5f, "Item Spawn Point");
    }

    // Method to draw an arrowhead at the end of the arrow
    private void DrawArrowhead(Vector3 position, Vector3 direction)
    {
        // Calculate left and right positions for the arrowhead
        Vector3 left = position + Quaternion.Euler(0, 0, arrowHeadAngle) * -direction * arrowHeadLength;
        Vector3 right = position + Quaternion.Euler(0, 0, -arrowHeadAngle) * -direction * arrowHeadLength;

        // Draw the lines for the arrowhead
        Gizmos.DrawLine(position, left);
        Gizmos.DrawLine(position, right);
    }

    // Method to draw the label with a dark background
    private void DrawLabelWithBackground(Vector3 position, string label)
    {
        #if UNITY_EDITOR
        // Set text color
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = labelTextColor;
        labelStyle.fontSize = 15;

        // Calculate the label size
        Vector2 labelSize = labelStyle.CalcSize(new GUIContent(label));

        // Create a background rectangle for the label
        Rect labelRect = new Rect(position.x - labelSize.x / 2, position.y, labelSize.x, labelSize.y);

        // Set up the background color for the label
        Handles.BeginGUI(); // Begin GUI section for drawing 2D elements
        GUI.backgroundColor = labelBackgroundColor; // Set the background color for the label

        // Draw the background rectangle
        GUI.Box(labelRect, GUIContent.none);

        // Draw the label on top of the background
        Handles.Label(position, label, labelStyle);

        Handles.EndGUI(); // End GUI section
        #endif
    }
}
