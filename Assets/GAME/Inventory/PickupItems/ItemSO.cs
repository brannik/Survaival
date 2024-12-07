using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;
[CreateAssetMenu(menuName = "Game/Items/New item")]
public class ItemSO : ScriptableObject
{
    public int itemId;
    public string itemName;
    public ItemSubtype subType;
    public string itemDescription;
    public int maxStack;
    public Sprite itemSprite;
    public PickupType pickupType;

    public Quality quality;
}
