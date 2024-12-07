using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class Set_Quality : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown qualityDropdown;
    private const string QualityLevelKey = "GraphicsQualityLevel";

    private void Start()
    {
        // Populate the dropdown with quality levels
        PopulateDropdown();

        // Load and apply saved quality level, or default to the current one
        int savedQualityLevel = PlayerPrefs.GetInt(QualityLevelKey, QualitySettings.GetQualityLevel());
        SetQualityLevel(savedQualityLevel);
        qualityDropdown.value = savedQualityLevel;

        // Add listener for dropdown changes
        qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
    }

    private void PopulateDropdown()
    {
        qualityDropdown.options.Clear();

        // Add each quality level name to the dropdown
        foreach (string levelName in QualitySettings.names)
        {
            qualityDropdown.options.Add(new TMP_Dropdown.OptionData(levelName));
        }

        qualityDropdown.RefreshShownValue();
    }

    private void SetQualityLevel(int index)
    {
        // Set the quality level based on dropdown selection
        QualitySettings.SetQualityLevel(index, true);

        // Save the selected quality level
        PlayerPrefs.SetInt(QualityLevelKey, index);
        PlayerPrefs.Save();

        Debug.Log($"Graphics quality changed to: {QualitySettings.names[index]} (saved)");
    }
}
