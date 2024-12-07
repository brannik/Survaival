using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorWindow : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI ErrorTitle;
    [SerializeField] public TextMeshProUGUI ErrorText;
    [SerializeField] public Button BTN_Close;
     // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        BTN_Close.onClick.AddListener(delegate{
            gameObject.SetActive(false);
        });
    }

    public void ShowError(string _title,string _text){
        ErrorTitle.text = _title;
        ErrorText.text = _text;
        gameObject.SetActive(true);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.warning);
    }
}
