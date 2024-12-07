using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListElement : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI TXT_LobbyName;
    [SerializeField] public TextMeshProUGUI TXT_PlayersCount;

    public void InitData(string _lName,string _plrCount,string _lid){
        TXT_LobbyName.text = _lName;
        TXT_PlayersCount.text = _plrCount;
        gameObject.GetComponent<Button>().onClick.AddListener(delegate{
            LobbyManager lMan = FindAnyObjectByType<LobbyManager>();
            lMan.JoinLobbyById(_lid);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonPressed);
        });
    }
}
