using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static ENUMS;
public class Inventory : MonoBehaviour
{
    [SerializeField] public GameObject InventoryUI;
    private int slots = 60;
    [SerializeField] private Transform inventoryContent;
    [SerializeField] private GameObject emptyInventorySlot;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI slotsText;
    [SerializeField] private InfoWindow infoWindow;
    private int money = 0;
    private InventorySlot[] inventorySlots;


    void Awake(){
        inventorySlots = new InventorySlot[slots];
        for(int i=0;i<slots;i++){
            inventorySlots[i] = new InventorySlot(null,i);
        }
        InventoryUI.SetActive(false);
        infoWindow.gameObject.SetActive(false);
        UpdateUI();
    }

    #region BUILDIG_SYSTEM_HELPERS

    public int GetItemAmountFromInventory(ItemSO item){
        int foundAmount = 0;
        foreach(InventorySlot slot in inventorySlots){
            if(slot.GetItem() == item){
                foundAmount += slot.GetAmount();
            }
        }
        return foundAmount;
    }
    public void RemoveItems(ItemSO item,int totalAmount){
        int leftover = totalAmount;
        for(int i=0;i<inventorySlots.Length;i++){
            if(inventorySlots[i].GetItem() == item && inventorySlots[i].GetAmount() > leftover){
                inventorySlots[i].SetAmount(inventorySlots[i].GetAmount() - leftover);
                break;
            }else if(inventorySlots[i].GetItem() == item && inventorySlots[i].GetAmount() <= leftover){
                leftover = leftover - inventorySlots[i].GetAmount();
                inventorySlots[i] = new InventorySlot(null,0);
            }
            
        }
        UpdateUI();
    }

    #endregion

    #region ADD_ITEMS
    public bool AddItem(ItemSO item, int amount)
    {
        //print($"Addind item {item.itemName} amount {amount}");
        if(item.pickupType == PickupType.Money){
            AudioManager.Instance.PlaySFX(AudioManager.Instance.coinPickup);
            money += amount;
            UpdateUI();
            return true;
        }
        if(item.pickupType == PickupType.Item){
            AudioManager.Instance.PlaySFX(AudioManager.Instance.armorPickup);
            foreach (InventorySlot slot in inventorySlots)
            {
                // Check for either an empty slot or a slot with the same item
                if (slot.IsEmptySlot() || slot.GetItem() == item)
                {
                    int maxStack = item.maxStack; // Maximum stack size for the item
                    int currentAmount = slot.GetAmount(); // Current amount in the slot
            
                    if (currentAmount + amount <= maxStack)
                    {
                        // If the total fits in this slot, update and exit
                        slot.UpdateSlot(item, currentAmount + amount);
                        UpdateUI();
                        return true; 
                    }
                    else
                    {
                        // Fill the slot to its max and reduce `amount`
                        int fillAmount = maxStack - currentAmount;
                        slot.UpdateSlot(item, maxStack);
                        amount -= fillAmount;
                    }
                }
            }

            // If there's still some amount left, try to add to empty slots
            foreach (InventorySlot slot in inventorySlots)
            {
                if (slot.IsEmptySlot())
                {
                    int maxStack = item.maxStack;
                    if (amount <= maxStack)
                    {
                        slot.UpdateSlot(item, amount);
                        UpdateUI();
                        return true;
                    }
                    else
                    {
                        // Fill the empty slot and reduce `amount`
                        slot.UpdateSlot(item, maxStack);
                        amount -= maxStack;
                    }
                }
            }

            // Optionally: Handle leftover `amount` that can't fit in the inventory
            if (amount > 0)
            {
                Debug.LogWarning($"Not enough space for {amount} more {item.name}(s)");
            
            }
            UpdateUI(); // Update UI after making changes
            return false;
        }
        return false;
        
    }

    public void UpdateUI(){
        if(inventoryContent.childCount > 0){
            foreach(Transform child in inventoryContent){
                Destroy(child.gameObject);
            }
        }
        for(int i=0;i<inventorySlots.Length;i++){
            var a = Instantiate(emptyInventorySlot,inventoryContent);
            inventorySlots[i].SetId(i);
            a.GetComponent<InventoryElement>().SetData(inventorySlots[i].GetAmount(),inventorySlots[i].GetItem(),i);
            a.GetComponent<SlotHandler>().SlotId = inventorySlots[i].GetId();
            if(!inventorySlots[i].IsEmptySlot()){
                a.GetComponent<InventoryElement>().ShowItem();
            }
        }

        moneyText.text = $"{money} g";
        slotsText.text = $"{GetEmptySlots()}/{slots}";
    }
    private int GetEmptySlots(){
        int i = 0;
        foreach(InventorySlot slot in inventorySlots){
            if(slot.IsEmptySlot()) i++;
        }
        return i;
    }

    #endregion

    #region MOVE_ITEMS

    public void SwapSlots(int movedSlot,int staticSlot){
        //print($"drop slot {movedSlot} over slot {staticSlot}");
        if(inventorySlots[staticSlot].IsEmptySlot()){
            UpdateUI();
        }else{
            InventorySlot tmpSlot = GetSlotById(movedSlot);
            inventorySlots[movedSlot] = inventorySlots[staticSlot];
            inventorySlots[staticSlot] = tmpSlot;

            UpdateUI();
        }
        

    }

    private bool SlotIsEmpty(int id){
        foreach(InventorySlot slot in inventorySlots){
            if(slot.GetId() == id && slot.IsEmptySlot()){
               return true; 
            }
        }
        return false;
    }
    private InventorySlot GetSlotById(int id){
        foreach(InventorySlot slot in inventorySlots){
            if(slot.GetId() == id){
               return slot; 
            }
        }
        return null;
    }
    #endregion

    #region DROP_ITEMS

    #endregion

    #region UI_MANIPULATION
    public void ToggleOnInfoWindow(string title,string subtype,string descr,Sprite img,int maxstack,string itemQuality,string qualityColor){
        infoWindow.SetData(title,subtype,descr,img,maxstack,itemQuality,qualityColor);
        infoWindow.gameObject.SetActive(true);
    }
    public void ToggleOffInfoWindow(){
        infoWindow.gameObject.SetActive(false);
    }
    #endregion
}
