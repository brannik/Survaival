using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewLobbyWindow : MonoBehaviour
{
    LobbyManager lmgr;
    [SerializeField] public TMP_InputField LobbyName;
    [SerializeField] public TMP_InputField MaxPlayers;
    [SerializeField] public Toggle PrivateMark;
    [SerializeField] public Button BTN_Create;
    [SerializeField] public Button BTN_Cancel;
    private bool nameEmpty = true;
    private bool maxPlayersEmpty = true;
    // Start is called before the first frame update
    void Start()
    {
        lmgr = FindAnyObjectByType<LobbyManager>();
        BTN_Create.onClick.AddListener(delegate{
            SendCreateLobby();
            ResetUI();
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonPressed);
        });
        BTN_Cancel.onClick.AddListener(delegate{
            ResetUI();
            gameObject.SetActive(false);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonPressed);
        });

        

        LobbyName.onValueChanged.AddListener(delegate{
            if(LobbyName.text.Length >= 1){
                LobbyName.GetComponent<Image>().color = Color.white;
                LobbyName.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
                nameEmpty = false;
            }else{
                LobbyName.GetComponent<Image>().color = Color.red;
                LobbyName.GetComponentInChildren<TextMeshProUGUI>().color = Color.yellow;
                LobbyName.GetComponentInChildren<TextMeshProUGUI>().text = "EMPTY";
                nameEmpty = true;
            }
        });

        MaxPlayers.onValueChanged.AddListener(delegate{
            if(MaxPlayers.text.Length >= 1){
                MaxPlayers.GetComponent<Image>().color = Color.white;
                MaxPlayers.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
                maxPlayersEmpty = false;
            }else{
                MaxPlayers.GetComponent<Image>().color = Color.red;
                MaxPlayers.GetComponentInChildren<TextMeshProUGUI>().color = Color.yellow;
                MaxPlayers.GetComponentInChildren<TextMeshProUGUI>().text = "EMPTY";
                maxPlayersEmpty = true;
            }
        });

        ResetUI();
    }

    private void SendCreateLobby(){
        if(MaxPlayers.text.Length >= 1){
            MaxPlayers.GetComponent<Image>().color = Color.white;
            MaxPlayers.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
            maxPlayersEmpty = false;
        }else{
            MaxPlayers.GetComponent<Image>().color = Color.red;
            MaxPlayers.GetComponentInChildren<TextMeshProUGUI>().color = Color.yellow;
            MaxPlayers.GetComponentInChildren<TextMeshProUGUI>().text = "EMPTY";
            maxPlayersEmpty = true;
        }
        if(LobbyName.text.Length >= 1){
            LobbyName.GetComponent<Image>().color = Color.white;
            LobbyName.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
            nameEmpty = false;
        }else{
            LobbyName.GetComponent<Image>().color = Color.red;
            LobbyName.GetComponentInChildren<TextMeshProUGUI>().color = Color.yellow;
            LobbyName.GetComponentInChildren<TextMeshProUGUI>().text = "EMPTY";
            nameEmpty = true;
        }

        if(!nameEmpty && !maxPlayersEmpty){
            lmgr.CreateNewLobby(LobbyName.text,MaxPlayers.text,PrivateMark.isOn);
            LobbyName.text = "";
            MaxPlayers.text = "";
            PrivateMark.isOn = false;
            gameObject.SetActive(false);
        }
        
    }
    private void ResetUI(){
        LobbyName.text = "";
        MaxPlayers.text = "";
        PrivateMark.isOn = false;
        LobbyName.GetComponent<Image>().color = Color.white;
        LobbyName.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        LobbyName.GetComponentInChildren<TextMeshProUGUI>().text = "Lobby Name";
        MaxPlayers.GetComponent<Image>().color = Color.white;
        MaxPlayers.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        MaxPlayers.GetComponentInChildren<TextMeshProUGUI>().text = "Max Players";

        nameEmpty = true;
        maxPlayersEmpty = true;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
