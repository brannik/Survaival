using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InteractionButton : MonoBehaviour
{
    [SerializeField] public GameObject mainUI;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private GameObject letterBG;
    [SerializeField] private GameObject actionBG;
    [SerializeField] private TextMeshProUGUI keybindLetter;
    [SerializeField] private TextMeshProUGUI requiredAction;

    void Awake(){
        mainUI.SetActive(false);
    }
   
    public void ShowUI(string _requiredAction,string _letter){
        loadingBar.gameObject.SetActive(false);
        loadingBar.value = 0f;
        requiredAction.text = _requiredAction;
        keybindLetter.text = _letter;
        letterBG.SetActive(true);
        actionBG.SetActive(true);
        mainUI.SetActive(true);
    }
    public void HideUI(){
        loadingBar.value = 0f;
        loadingBar.gameObject.SetActive(false);
        letterBG.SetActive(false);
        actionBG.SetActive(false);
    }
    public void LoadinUI()
    {
        loadingBar.gameObject.SetActive(true);
    }
    public void UpdateData(float minValue,float maxValue){
        
        loadingBar.value = Mathf.Clamp01(minValue / maxValue);
        letterBG.SetActive(false);
        actionBG.SetActive(false);
    }

}
