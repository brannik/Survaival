using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : NetworkBehaviour
{
    [SerializeField] public GameObject menuUI;
    [SerializeField] public GameObject playerUI;
    [SerializeField] private Button BTN_Resume;
    [SerializeField] private Button BTN_Exit;

    private LobbyManager lobbyManager;

    //PlayerNetwork player;

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) gameObject.SetActive(false);
        lobbyManager = FindAnyObjectByType<LobbyManager>();
    }

    void Awake(){
        menuUI.SetActive(false);
        playerUI.SetActive(true);
        //player = FindAnyObjectByType<PlayerNetwork>();
    }
    void OnEnable(){
        InitUIManager.Instance.SetCursors(gameObject);
        BTN_Resume.onClick.AddListener(delegate{
            menuUI.SetActive(false);
            playerUI.SetActive(true);
            //player.EnablePlayer();
        });
        BTN_Exit.onClick.AddListener(delegate{
            menuUI.SetActive(false);
            NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetActiveScene());
            lobbyManager.LeaveLobby();
            InitUIManager.Instance.ShowUI();
            Debug.Log("Exit to main scene");
        });
    }
    void Update()
    {
        if(!IsOwner) return;
    }

    
}
