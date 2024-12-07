using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ENUMS;
public class InventoryElement : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler,IBeginDragHandler,IEndDragHandler,IDragHandler
{
    [SerializeField] public TextMeshProUGUI stackCount;
    [SerializeField] public Image backColor;
    [SerializeField] public Image itemBackground;
    [SerializeField] public Image itemSprite;
    private Inventory inventory;
    public ItemSO item;
    public int amount;
    private int mySlotId; // store old slotId to be able to swap slots
    public int MySlotId{
        set { mySlotId = value;}
        get { return mySlotId;}
    }
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector3 originalPosition;

    private float originalZPosition;

    void Awake(){
        inventory = FindAnyObjectByType<Inventory>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        originalZPosition = rectTransform.position.z;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(item == null) return;
        string qualityColor = QualityColors[(int)item.quality];
        if (ColorUtility.TryParseHtmlString(qualityColor, out Color hexColor))
        {
            backColor.color = hexColor;
            Color c = backColor.color;
            c.a = Mathf.Clamp01(1f);
            backColor.color = c;
        }
        string color = QualityColors[(int)item.quality];
        inventory.ToggleOnInfoWindow(item.itemName,item.subType.ToString(),item.itemDescription,item.itemSprite,item.maxStack,item.quality.ToString(),color);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(item == null) return;
        string qualityColor = QualityColors[(int)item.quality];
        if (ColorUtility.TryParseHtmlString(qualityColor, out Color hexColor))
        {
            backColor.color = hexColor;
            Color c = backColor.color;
            c.a = Mathf.Clamp01(0.3f);
            backColor.color = c;
        }
        inventory.ToggleOffInfoWindow();
    }

    public void SetData(int stack,ItemSO item,int slotId){
        this.item = item;
        amount = stack;
        mySlotId = slotId;
        itemBackground.gameObject.SetActive(false);
        backColor.gameObject.SetActive(false);
    }
    public void ShowItem(){
        itemBackground.gameObject.SetActive(true);
        backColor.gameObject.SetActive(true);
        string qualityColor = QualityColors[(int)item.quality];
        if (ColorUtility.TryParseHtmlString(qualityColor, out Color hexColor))
        {
            backColor.color = hexColor;
            Color c = backColor.color;
            c.a = Mathf.Clamp01(0.3f);
            backColor.color = c;
        }
        
        itemSprite.sprite = this.item.itemSprite;
        stackCount.text = amount.ToString();
    }


    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        // Make the item semi-transparent while dragging
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;  // Prevent blocking raycasts from other UI elements
        }

        // Move the dragged item to the front visually (i.e., the last sibling in the hierarchy)
        Vector3 newPosition = rectTransform.position;
        newPosition.z = 10f;  // Increase Z to move the item in front of others
        rectTransform.position = newPosition;

        AllowPointerLock.Instance.IsHoldingItem = true;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        Vector3 originalPos = rectTransform.position;
        originalPos.z = originalZPosition;  // Restore Z position
        rectTransform.position = originalPos;
        AllowPointerLock.Instance.IsHoldingItem = false;
        inventory.UpdateUI();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(item == null) return;
        Vector2 newPosition = rectTransform.anchoredPosition + eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
        rectTransform.anchoredPosition = newPosition;
    }
}
