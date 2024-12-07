using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ChanceSliderAttribute))]
public class ChanceSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ChanceSliderAttribute slider = (ChanceSliderAttribute)attribute;

        EditorGUI.BeginProperty(position, label, property);

        // Split the position into two parts: one for the slider, one for the label
        Rect sliderRect = new Rect(position.x, position.y, position.width - 30, position.height); // Slider takes most of the space
        Rect labelRect = new Rect(position.x + position.width - 25, position.y, 25, position.height); // Small space for the "%"

        // Draw the slider and snap the value to the defined step
        float rawValue = EditorGUI.Slider(sliderRect, label, property.floatValue, slider.Min, slider.Max);
        property.floatValue = Mathf.Round(rawValue / slider.Step) * slider.Step;

        // Draw the "%" label next to the slider
        EditorGUI.LabelField(labelRect, "%");

        EditorGUI.EndProperty();
    }
}