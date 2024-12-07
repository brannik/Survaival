using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Custom class to handle PointerEnter and PointerExit for Button
public class ButtonPointerEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;

    // Set the Button reference
    public void SetButton(Button btn)
    {
        button = btn;
        button.onClick.AddListener(PlayClickSound);
    }

    // This method is called when the cursor enters the Button
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorManager.Instance.GetModelByName("button"));
    }

    // This method is called when the cursor exits the Button
    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorManager.Instance.GetModelByName("default"));
    }
    private void PlayClickSound()
    {
        // Play the click sound effect when the button is clicked
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonPressed);
    }
}
