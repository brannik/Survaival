using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for Pointer Event Handlers

public class SliderPointerEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Slider slider;

    // Set the Slider reference
    public void SetSlider(Slider slider)
    {
        this.slider = slider;
    }

    // Called when the pointer enters the slider
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorManager.Instance.GetModelByName("slider"));
        // Optional: Change the cursor style or play a hover sound effect
        // Example: AudioManager.Instance.PlaySFX(AudioManager.Instance.sliderHover);
    }

    // Called when the pointer exits the slider
    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorManager.Instance.GetModelByName("default"));
        // Optional: Reset cursor style or stop hover sound
    }
}
