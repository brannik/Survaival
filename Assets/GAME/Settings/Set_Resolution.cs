using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Set_Resolution : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resDropDown;
    [SerializeField] private Toggle fullScreenToggle;
    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;
    private float currentRefreshRate;
    private int currentResolutionIndex = 0;
    // Start is called before the first frame update
    private const string ResolutionKey = "ResolutionIndex";
    private const string FullscreenKey = "Fullscreen";

    void Start()
    {
        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();
        resDropDown.ClearOptions();
        currentRefreshRate = (int)Math.Ceiling(Screen.currentResolution.refreshRateRatio.value);

        // Filter resolutions by current refresh rate
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].refreshRateRatio.value == currentRefreshRate)
            {
                filteredResolutions.Add(resolutions[i]);
            }
        }

        // Create dropdown options
        List<string> options = new List<string>();
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string resolutionOption = filteredResolutions[i].width + "x" + filteredResolutions[i].height + " " + filteredResolutions[i].refreshRateRatio.value + " Hz";
            options.Add(resolutionOption);

            // Check if this resolution matches the current screen resolution
            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resDropDown.AddOptions(options);

        // Load saved settings
        LoadSettings();

        resDropDown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = filteredResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, fullScreenToggle.isOn);

        // Save settings
        SaveSettings(resolutionIndex, fullScreenToggle.isOn);

        Debug.Log($"Resolution set to: {resolution.width}x{resolution.height}, Fullscreen: {fullScreenToggle.isOn}");
    }

    private void SaveSettings(int resolutionIndex, bool isFullscreen)
    {
        PlayerPrefs.SetInt(ResolutionKey, resolutionIndex);
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("Settings saved.");
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey(ResolutionKey))
        {
            currentResolutionIndex = PlayerPrefs.GetInt(ResolutionKey);
        }

        if (PlayerPrefs.HasKey(FullscreenKey))
        {
            fullScreenToggle.isOn = PlayerPrefs.GetInt(FullscreenKey) == 1;
        }

        // Apply the saved resolution and fullscreen mode
        resDropDown.value = currentResolutionIndex;
        SetResolution(currentResolutionIndex);
    }
}
