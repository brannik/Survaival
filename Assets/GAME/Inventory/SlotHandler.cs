using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements.Experimental;

public class SlotHandler : MonoBehaviour,IDropHandler
{
    private int slotId;
    private Inventory inventory;

    void Awake(){
        inventory = GetComponentInParent<Inventory>();
    }
    public int SlotId{
        set { slotId = value;}
        get { return slotId;}
    }

    public void OnDrop(PointerEventData eventData)
    {
        AllowPointerLock.Instance.IsHoldingItem = false;
        if(eventData.pointerDrag != null){
            inventory.SwapSlots(eventData.pointerDrag.gameObject.GetComponent<InventoryElement>().MySlotId,SlotId);
            
        }
    }
}
