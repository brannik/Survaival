using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Ensure you have the TextMeshPro namespace

public class TMP_InputFieldHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_InputField inputField;

    // Set the TMP_InputField reference
    public void SetInputField(TMP_InputField field)
    {
        inputField = field;

    }

    // This method is called when the cursor enters the TMP_InputField
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorManager.Instance.GetModelByName("input"));
       
    }

    // This method is called when the cursor exits the TMP_InputField
    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorManager.Instance.GetModelByName("default"));
    }

}
