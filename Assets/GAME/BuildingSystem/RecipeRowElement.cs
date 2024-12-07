using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeRowElement : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI requiredAmount;
    [SerializeField] private Image doneStatus; 
    [SerializeField] private Sprite amountOk;
    [SerializeField] private Sprite amountFail;

    public void InitData(ItemSO item, int amount,bool isDone){
        itemIcon.sprite = item.itemSprite;
        itemName.text = item.itemName;
        requiredAmount.text = amount.ToString();
        if(isDone){
            doneStatus.sprite = amountOk;
        }else{
            doneStatus.sprite = amountFail;
        }
    }
 }
