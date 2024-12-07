using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCustomization : MonoBehaviour
{
    [Header("Name")]
    [SerializeField] private TMP_InputField characterName;
    [SerializeField] private GameObject playerModel;
    [Header("Customizations")]
    [SerializeField] private SO_Colors colors;
    
    public float rotationIncrement = 25f; // Rotation angle in degrees
    [Header("Underwear")]
    [SerializeField] public GameObject UnderwearObject;
    private Color[] underwearColors;
    [SerializeField] public Image underwearColorVisualization;
    private int currentUnderwearColorIndex = 0;
    [Header("Hair")]
    [SerializeField] public GameObject HairObject;
    private Color[] hairCollors;
    [SerializeField] public Image hairColorVisualization;
    private int currentHairColorIndex = 0;
    
    void OnEnable(){
        underwearColors = colors.colors;
        hairCollors = colors.colors;
        LoadColors();
        UnderwearSetColor();
        HairSetColor();
    }
    public void RotateLeft()
    {
        if (playerModel != null)
        {
            playerModel.transform.Rotate(0f, -rotationIncrement, 0f);
        }
        else
        {
            Debug.LogWarning("Target object is not assigned!");
        }
    }
    // Rotate the object to the right (positive Y-axis)
    public void RotateRight()
    {
        if (playerModel != null)
        {
            playerModel.transform.Rotate(0f, rotationIncrement, 0f);
        }
        else
        {
            Debug.LogWarning("Target object is not assigned!");
        }
    }

    public void UnderwearSetNextColor()
    {
        if (underwearColors.Length == 0)
        {
            Debug.LogWarning("No colors in the color list.");
            return;
        }

        // Increment the color index, loop back to 0 if it exceeds the array length
        currentUnderwearColorIndex = (currentUnderwearColorIndex + 1) % underwearColors.Length;

        // Set the new color
        UnderwearSetColor();
    }

    // Function to set the previous color in the list
    public void UnderwearSetPreviousColor()
    {
        if (underwearColors.Length == 0)
        {
            Debug.LogWarning("No colors in the color list.");
            return;
        }

        // Decrement the color index, loop back to the last index if it goes below 0
        currentUnderwearColorIndex = (currentUnderwearColorIndex - 1 + underwearColors.Length) % underwearColors.Length;

        // Set the new color
        UnderwearSetColor();
    }

    // Function to apply the current color to the target object
    private void UnderwearSetColor()
    {
        if (UnderwearObject != null)
        {
            Renderer objectRenderer = UnderwearObject.GetComponent<Renderer>();

            if (objectRenderer != null)
            {
                objectRenderer.material.color = underwearColors[currentUnderwearColorIndex];
                underwearColorVisualization.color = underwearColors[currentUnderwearColorIndex];
            }
            else
            {
                Debug.LogWarning("No Renderer found on the target object.");
            }
        }
        else
        {
            Debug.LogWarning("Target object is not assigned!");
        }
    }

    public void HairSetNextColor()
    {
        if (hairCollors.Length == 0)
        {
            Debug.LogWarning("No colors in the color list.");
            return;
        }

        // Increment the color index, loop back to 0 if it exceeds the array length
        currentHairColorIndex = (currentHairColorIndex + 1) % hairCollors.Length;

        // Set the new color
        HairSetColor();
    }

    // Function to set the previous color in the list
    public void HairSetPreviousColor()
    {
        if (hairCollors.Length == 0)
        {
            Debug.LogWarning("No colors in the color list.");
            return;
        }

        // Decrement the color index, loop back to the last index if it goes below 0
        currentHairColorIndex = (currentHairColorIndex - 1 + hairCollors.Length) % hairCollors.Length;

        // Set the new color
        HairSetColor();
    }

    // Function to apply the current color to the target object
    private void HairSetColor()
    {
        if (HairObject != null)
        {
            Renderer objectRenderer = HairObject.GetComponent<Renderer>();

            if (objectRenderer != null)
            {
                objectRenderer.material.color = hairCollors[currentHairColorIndex];
                hairColorVisualization.color = hairCollors[currentHairColorIndex];
            }
            else
            {
                Debug.LogWarning("No Renderer found on the target object.");
            }
        }
        else
        {
            Debug.LogWarning("Target object is not assigned!");
        }
    }

    public void SaveColors(){
        PlayerPrefs.SetInt("underwear_color",currentUnderwearColorIndex);
        PlayerPrefs.SetInt("hair_color",currentHairColorIndex);
        PlayerPrefs.SetString("char_name",characterName.text);
        PlayerPrefs.Save();
    }

    private void LoadColors()
    {
        currentUnderwearColorIndex = PlayerPrefs.GetInt("underwear_color", 0);
        currentHairColorIndex = PlayerPrefs.GetInt("hair_color", 0);
        characterName.text = PlayerPrefs.GetString("char_name","EMPTY");
    }
}
