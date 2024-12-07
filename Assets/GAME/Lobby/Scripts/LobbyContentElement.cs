using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class LobbyContentElement : MonoBehaviour
{
    [SerializeField] public Image ReadyStatus;
    [SerializeField] public Image OwnerIcon;
    [SerializeField] public TextMeshProUGUI PlayerName;
    [SerializeField] public Button BTN_Mute;
    [SerializeField] public Button Btn_Kick;

    private string PlayerId;
    void OnEnable(){
        Btn_Kick.onClick.AddListener(delegate{
            LobbyManager lmgr = FindAnyObjectByType<LobbyManager>();
            lmgr.KickPlayer(PlayerId);
        });
        BTN_Mute.onClick.AddListener(delegate{
            Debug.Log($"Mute player {PlayerId}");
        });
    }

    public void InitData(string _plrId,bool _ready,bool _owner,string _name){
        PlayerId = _plrId;
        ReadyStatus.gameObject.SetActive(_ready);
        OwnerIcon.gameObject.SetActive(_owner);
        Btn_Kick.gameObject.SetActive(_owner);
        BTN_Mute.gameObject.SetActive(false);
        PlayerName.text = _name;
    }
}
