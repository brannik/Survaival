using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionbarButtonsRight : MonoBehaviour
{
    public void ToggleInventory(){
        Inventory inventory = GetComponentInParent<PlayerController>().inventory;
        inventory.InventoryUI.SetActive(!inventory.InventoryUI.activeSelf);
    }
    public void ToggleCrafting(){
        GameObject buildingUI = GetComponentInParent<PlayerController>().BuildingUI;
        buildingUI.SetActive(!buildingUI.activeSelf);
    }
}
