using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class InfoWindow : MonoBehaviour
{
    [SerializeField] private Image border;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI itemSubType;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI maxStackText;

    public void SetData(string _name,string subType,string _descr, Sprite _img,int _maxStack,string quality,string qualityColor){
        titleText.text = _name;
        itemSubType.text = $"{quality}  {subType}";
        if (ColorUtility.TryParseHtmlString(qualityColor, out Color hexColor))
        {
            itemSubType.color = hexColor; // Set to hex-defined orange
            border.color = hexColor;
        }
        description.text = _descr;
        itemIcon.sprite = _img;
        maxStackText.text = $"Max Stack: {_maxStack}";
    }
}
