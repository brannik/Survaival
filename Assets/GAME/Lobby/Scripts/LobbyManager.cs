using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public Lobby currentLobby =null;
    QueryResponse lobbyList;
    private GameObject ActiveWindow;
    private float hearthbeatTimer = 15f;
    private float lobbyContentUpdateTimer = 10.0f;
    private float lobbyListUpdateTimer = 10.0f;
    private string playerId;
    string joinCode;
    string relayCode;
    int playersNumber = 0;

    RelayServerData relayServerDataHost;
    RelayServerData relayServerData;

    StringBuilder DEBUG_HOST = new StringBuilder();
    StringBuilder DEBUG_CLIENT = new StringBuilder();

    float ButtonReactivateDelay = 1f;

    InitUIManager UIManager;
    [Header("UI")]
    [SerializeField] public GameObject ListUI;
    [SerializeField] public GameObject LobbyContentUI;
    [SerializeField] public GameObject NewLobbyUI;
    [Header("Lobby List")]
    [SerializeField] public GameObject LobbyListContent;
    [SerializeField] public GameObject ListPrefab;
    [SerializeField] public Button BTN_CreateLobby;
    [SerializeField] public Button BTN_JoinByCode;
    [SerializeField] public TextMeshProUGUI RefreshText;
    [SerializeField] public TMP_InputField INP_LobbyCode;
    [Header("Lobby Content")]
    [SerializeField] public GameObject LobbyContentParent;
    [SerializeField] public GameObject ContentPrefab;
    [SerializeField] public Button BTN_LeaveLobby;
    [SerializeField] public Button BTN_StartTheGame;
    [SerializeField] public TextMeshProUGUI LobbyName;
    [SerializeField] public TextMeshProUGUI LobbyCode;
    [SerializeField] public TextMeshProUGUI PlayersCount;
    // Start is called before the first frame update
    
    async void Awake()
	{
		try
		{
			await UnityServices.InitializeAsync();
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}

	}
    
    void Start()
    {
        ShowLobbyListUI();
        BTN_StartTheGame.gameObject.SetActive(false);
        BTN_JoinByCode.onClick.AddListener(delegate{
            if(INP_LobbyCode.text.Length != 6){
                INP_LobbyCode.GetComponent<Image>().color = Color.red;
                INP_LobbyCode.GetComponentInChildren<TextMeshProUGUI>().color = Color.yellow;
            }else{
                JoinLobbyByCode(INP_LobbyCode.text);
            }
            BTN_JoinByCode.interactable = false;
            StartCoroutine(EnableButtonAfterDelay(BTN_JoinByCode, ButtonReactivateDelay));
        });
        INP_LobbyCode.onValueChanged.AddListener(delegate{
            if(INP_LobbyCode.text.Length != 6){
                INP_LobbyCode.GetComponent<Image>().color = Color.red;
                INP_LobbyCode.GetComponentInChildren<TextMeshProUGUI>().color = Color.yellow;
            }else{
                INP_LobbyCode.GetComponent<Image>().color = Color.white;
                INP_LobbyCode.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
            }

            
        });
        BTN_CreateLobby.onClick.AddListener(delegate{
            ShowNewLobbyUI();
            BTN_CreateLobby.interactable = false;
            StartCoroutine(EnableButtonAfterDelay(BTN_CreateLobby, ButtonReactivateDelay));
        });
        BTN_LeaveLobby.onClick.AddListener(delegate{
            LeaveLobby();
            BTN_LeaveLobby.interactable = false;
            StartCoroutine(EnableButtonAfterDelay(BTN_LeaveLobby, ButtonReactivateDelay));
        });
        
        //UpateLobbyList();
        playerId = AuthenticationService.Instance.PlayerId;
        InitUIManager.Instance.SetCursors(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        HandleHearthbeat();
        HandleLobbyContentUpdate();
        HandleLobbyListUpdate();
    }

    #region LOBBY
    public async void CreateNewLobby(string _name,string _maxPlayers,bool _private){
        // create lobby functionality
        // switch to lobby content since if autojoined
        BTN_StartTheGame.gameObject.SetActive(false);
        DEBUG_HOST.Append("begin create lobby ->");
        try{
            
            int.TryParse(_maxPlayers, out int mPlayers);
            playersNumber = mPlayers;
            CreateLobbyOptions options = new CreateLobbyOptions{
                IsPrivate = _private,
                Player = GetPlayer(),
                Data=new Dictionary<string, DataObject>{
                    {"IsGameStarted",new DataObject(DataObject.VisibilityOptions.Member,"false")},
                    {"RelayCode",new DataObject(DataObject.VisibilityOptions.Public,joinCode)}
                }
            };
            currentLobby = await LobbyService.Instance.CreateLobbyAsync(_name,mPlayers,options);
            LobbyHandler.Instance.joinedLobby = currentLobby;
            //Debug.Log($"Create lobby: Name - {_name} MaxPlayers: {_maxPlayers} - Private: {_private}");
            //Debug.LogWarning($"-> Created lobby with id: {currentLobby.Id}");
            DEBUG_HOST.Append($" lobby created: {currentLobby.Id}");
            BTN_StartTheGame.gameObject.SetActive(true);
            ShowLobbyContentUI();
        }catch(LobbyServiceException ex){
            Debug.Log(ex.Message);
        }
    }
    public async void JoinLobbyById(string id)
    {
        DEBUG_CLIENT.Append("-> JoinById ");

        try
        {
            // Join lobby
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(id, options);
            DEBUG_CLIENT.Append(" -> Successfully joined lobby");
            //await Task.Delay(500);
            LobbyHandler.Instance.joinedLobby = currentLobby;
            //await JoinRelay(_relayCode);

            // Show lobby content
            ShowLobbyContentUI();
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to join lobby: {ex.Message}");
        }
    }

    public async void JoinLobbyByCode(string lobbyCode){
        // join lobby functionality
        // switch windows to lobby content
        try{
            //JoinRelay(relayCode);
            lobbyList = await LobbyService.Instance.QueryLobbiesAsync();
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions{
                Player = GetPlayer()
            };
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode,options);
            
            ShowLobbyContentUI();
        }catch(LobbyServiceException ex){
            Debug.Log(ex.Message);
        }  
    }

    public async void LeaveLobby(){
        try{
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, playerId);
            currentLobby = null;
            ShowLobbyListUI();
        }catch(LobbyServiceException ex){
            Debug.Log(ex.Message);
        }
        
    }
    public async void KickPlayer(string plrId){
        try{
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, plrId);
            UpdateLobbyContent();
        }catch(LobbyServiceException ex){
            Debug.Log(ex.Message);
        }
    }

    public void DestroyLobby(){}

    private bool IsHost(){
        if(currentLobby != null && currentLobby.HostId == playerId){
            return true;
        }
        return false;
    }

    private bool IsGameStarted(){
        if(currentLobby != null){
            if(currentLobby.Data["IsGameStarted"].Value == "true"){
                return true;
            }
        }
        return false;
    }

    private async void HandleHearthbeat(){
        if(currentLobby != null && IsHost()){
            hearthbeatTimer -= Time.deltaTime;
            if(hearthbeatTimer <= 0){
                hearthbeatTimer = 15f;
                await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }
        }
    }

    private Player GetPlayer(){
        string PlayerName = PlayerPrefs.GetString("char_name");
        if(PlayerName == null || PlayerName == "")
            PlayerName = playerId;
        Player player = new Player{
            Data = new Dictionary<string, PlayerDataObject>{
                {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,PlayerName)}
            }
        };
        return player;
    }

    private bool IsInLobby(){
        foreach(Player _player in currentLobby.Players ){
            if(_player.Id == AuthenticationService.Instance.PlayerInfo.Id){
                return true;
            }
        }
        currentLobby = null;
        return false;
    }

    private async void EnterGame(){
        CursorManager.Instance.SetCursor(CursorManager.Instance.GetModelByName("default"));
        // load the scene and spawn other players
        if(!IsHost()){
            UIManager = FindAnyObjectByType<InitUIManager>();
            UIManager.HideUI();
            relayCode = currentLobby.Data["RelayCode"].Value;
            await JoinRelay(relayCode);
            Debug.LogWarning($"CLIENT: {DEBUG_CLIENT}");
            //NetworkManager.Singleton.LocalClientId
        }else{
            UIManager = FindAnyObjectByType<InitUIManager>();
            UIManager.HideUI();          
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerDataHost);
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("DemoLevel",LoadSceneMode.Single); 
            Debug.LogWarning($"HOST: {DEBUG_HOST}");
        }
        
        
    }

    #endregion

    #region RELAY

    private async Task CreateRelay(int _plrCount){
        try{
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(_plrCount-1);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            relayServerDataHost = new RelayServerData(allocation,"dtls");

            DEBUG_HOST.Append($" relay created: {joinCode}");
        }catch(RelayServiceException ex){
            Debug.Log(ex.Message);
        }
        print(DEBUG_HOST);
    }

    public async Task JoinRelay(string _relayCode)
    {
        
        try
        {
            Debug.Log($"Joining Relay with code: {_relayCode}");

            // Attempt to join relay using the provided RelayCode
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(_relayCode);
            //Debug.Log($"JoinAllocation successful. Allocation ID: {joinAllocation.AllocationId}");

            // Set Relay Server Data for UnityTransport
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
            // Start client connection
            NetworkManager.Singleton.StartClient();
            //Debug.Log("Client started and connected to the relay.");
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError($"Error joining relay: {ex.Message}");
        }
    }

    public async void StartTheGame(){
        
        if(currentLobby != null && IsHost()){
            BTN_StartTheGame.gameObject.SetActive(false);
            playersNumber = currentLobby.MaxPlayers;
            await CreateRelay(playersNumber+1);
            await Task.Delay(1000);
            try{
                UpdateLobbyOptions updateoptions = new UpdateLobbyOptions{
                    Data = new Dictionary<string, DataObject>{
                        {"IsGameStarted",new DataObject(DataObject.VisibilityOptions.Member,"true")},
                        { "RelayCode", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: joinCode) }
                    }
                };
                currentLobby = await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id,updateoptions);
                
                EnterGame();
            }catch(LobbyServiceException ex){
                Debug.Log(ex.Message);
            }
            
        }
    }

    #endregion

    #region UI_MANAGEMENT

    public async Task UpdateLobbyList()
    {
        // Clear previous list except protected elements
        foreach (Transform child in LobbyListContent.transform)
        {
            if (!child.gameObject.CompareTag("DONOT_DESTROY_ELEMENT"))
            {
            Destroy(child.gameObject);
            }
        }

        try
        {
            // Query all lobbies
            lobbyList = await LobbyService.Instance.QueryLobbiesAsync();
            
            await Task.Delay(500);

            foreach (Lobby _lobby in lobbyList.Results)
            {
                // Skip lobbies without the RelayCode key
                if (_lobby.Data == null || !_lobby.Data.ContainsKey("RelayCode"))
                {
                    Debug.Log($"Skipping lobby {_lobby.Name}: Missing RelayCode.");
                    continue;
                }

                // Instantiate the lobby element
                var el = Instantiate(ListPrefab, LobbyListContent.transform);

                // Populate the element with lobby data
                string name = _lobby.Name;
                string plr = $"{_lobby.MaxPlayers - _lobby.AvailableSlots} / {_lobby.MaxPlayers}";
                el.GetComponent<LobbyListElement>().InitData(name, plr, _lobby.Id);

                //Debug.Log($"Added lobby: {name} with RelayCode.");
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Error updating lobby list: {ex.Message}");
        }
    }

    private void UpdateLobbyContent(){
        if(IsInLobby()){
            // update the players list in current lobby
            if(LobbyContentParent.transform.childCount > 0){
                foreach(Transform child in LobbyContentParent.transform){
                    Destroy(child.gameObject);
                }
            }
            LobbyName.text = currentLobby.Name;
            LobbyCode.text = currentLobby.LobbyCode;
            PlayersCount.text = $"{currentLobby.MaxPlayers - currentLobby.AvailableSlots}/{currentLobby.MaxPlayers}";
            
            
            foreach(Player player in currentLobby.Players){
                var el = Instantiate(ContentPrefab,LobbyContentParent.transform);
                string name = player.Data["PlayerName"].Value;
                string plrId = player.Id;
                bool canKick = IsHost() && playerId != player.Id;
                bool isOwner = currentLobby.HostId == player.Id;
                el.GetComponent<LobbyContentElement>().InitData(plrId,isOwner,canKick,name);
                
            }
            

            if(IsHost()){
                
                BTN_StartTheGame.onClick.AddListener(delegate{
                    BTN_StartTheGame.interactable = false;
                    StartCoroutine(EnableButtonAfterDelay(BTN_StartTheGame, ButtonReactivateDelay));
                    StartTheGame();
                });
            }else{
                if(IsGameStarted()){
                    //relayCode = currentLobby.Data["RealyCode"].Value;
                    //JoinRelay();
                    EnterGame();
                    BTN_StartTheGame.gameObject.SetActive(true);
                    BTN_StartTheGame.GetComponentInChildren<TextMeshProUGUI>().text = "ENTER";
                    BTN_StartTheGame.onClick.AddListener(delegate{
                        BTN_StartTheGame.interactable = false;
                        StartCoroutine(EnableButtonAfterDelay(BTN_StartTheGame, ButtonReactivateDelay));
                        
                        EnterGame();
                    });
                    
                }else{
                    BTN_StartTheGame.gameObject.SetActive(false);
                    BTN_StartTheGame.onClick.RemoveAllListeners();
                }
            }
        }else{
            ShowLobbyListUI();
        }
    }
    private async void ShowLobbyListUI(){
        ActiveWindow = ListUI;
        ListUI.SetActive(true);
        LobbyContentUI.SetActive(false);
        NewLobbyUI.SetActive(false);

        INP_LobbyCode.text = "";
        INP_LobbyCode.GetComponent<Image>().color = Color.white;
        INP_LobbyCode.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        InitUIManager.Instance.SetCursors(ListUI);
        await UpdateLobbyList();
    }
    private void ShowNewLobbyUI(){
        NewLobbyUI.SetActive(true);
        InitUIManager.Instance.SetCursors(NewLobbyUI);
    }
    private void ShowLobbyContentUI(){

        ActiveWindow = LobbyContentUI;
        ListUI.SetActive(false);
        LobbyContentUI.SetActive(true);
        NewLobbyUI.SetActive(false);
        InitUIManager.Instance.SetCursors(LobbyContentUI);
        UpdateLobbyContent();
    }

    private async void HandleLobbyContentUpdate(){
        if(currentLobby != null){
            lobbyContentUpdateTimer -= Time.deltaTime;
            if(lobbyContentUpdateTimer <= 0){
                lobbyContentUpdateTimer = 2f;
                try{
                    if(IsInLobby()){
                        currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
                        UpdateLobbyContent();
                        relayCode = currentLobby.Data["RealyCode"].Value;
                    }
                }catch(LobbyServiceException ex){
                    Debug.Log(ex.Message);
                }
            }
        }
    }
    private async void HandleLobbyListUpdate(){
        if(currentLobby == null){
            lobbyListUpdateTimer -= Time.deltaTime;
            RefreshText.text = $"Refresh after {(lobbyListUpdateTimer).ToString("0")} s";
            if(lobbyListUpdateTimer <= 0){
                lobbyListUpdateTimer = 5.0f;
                try{
                    //lobbyList = await LobbyService.Instance.QueryLobbiesAsync();
                    await UpdateLobbyList();
                }catch(LobbyServiceException ex){
                    Debug.Log(ex.Message);
                }
            }
        }
    }

    #endregion

    IEnumerator EnableButtonAfterDelay(Button button, float seconds) {
        yield return new WaitForSeconds(seconds);
        button.interactable = true;
    }
}
