using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class BuildingObject : NetworkBehaviour
{
    [Header("Visualization in the editor")]
    [Range(0.1f, 10f)] public float width = 1f;
    [Range(0.1f, 10f)] public float height = 1f;
    [Range(0.1f, 10f)] public float depth = 1f;

    // Adjustable color for the gizmo
    public Color gizmoColor = Color.green;

    [Range(-5f, 5f)] public float offsetX = 0f;
    [Range(-5f, 5f)] public float offsetY = 0f;
    [Range(-5f, 5f)] public float offsetZ = 0f;

    [Range(2f, 50f)]public float textSize = 12f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private GameObject buildingUI; // use it to move the ui near the player
    private Vector3 originalUIPosition; // use to return the ui to original position
    [SerializeField] private float distanceInFront = 4.0f;

    [Header("Settings")]
    [SerializeField] private string buildingName = "none";
    [SerializeField] private AudioClip buildingSFX;
  

    // new values
    private Coroutine loadingCoroutine;
    private List<ulong> _playersToViewPreview;
    private ulong _thisObjectNetworkId;
    private GameObject _currentPreviewModel;
    private GameObject _currentModel;
    private int _currentLevel = 0;
    private Vector3 _originalScale;
    private float _buildTime;
    private bool _isBuilding = false;
    [Tooltip("Set to FALS for big objects to be able to move the ui near the player")]
    [SerializeField] private bool _staticUI = true;
    [SerializeField] private BuildingLevelsSO[] _buildingLevels;
    [Tooltip("Transperent model indicating there is a building")]
    [SerializeField] private Transform[] _buildingSpawnPoint;
    [Tooltip("If true - will NOT despewn previous levels of the building")]
    [SerializeField] private bool _overlapBuildings = true;

    private Dictionary<ulong, GameObject> previewModelsByNetworkId = new Dictionary<ulong, GameObject>();


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _thisObjectNetworkId = NetworkObject.NetworkObjectId;
            _playersToViewPreview = new List<ulong>();
            originalUIPosition = buildingUI.transform.position;
        }
        else
        {
            Debug.Log("Client-specific logic can go here.");
        }
    }


    void Start(){
        if (IsServer)
        {
            SpawnPreviewObject();
            UpdatePreviewForPlayers();
        }
        
    }
    void Update(){

        
    }
    private void UpdatePreviewForPlayers()
    {
        levelText.text = _currentLevel.ToString();
        messageText.text = "Upgrade";
        loadingBar.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        CustomLogger.Log($"Trigger entered by: {other?.name ?? "null"}", "BUILDING");
        CustomLogger.Log("Test Inventory", "INVENTORY");
        if (!IsServer)
        {
            CustomLogger.Log("Not the server. Exiting.", "BUILDING");
            return;
        }

        if (other == null)
        {
            CustomLogger.Log("Trigger entered by a null object.", "BUILDING");
            return;
        }

        // Cache the NetworkObject component
        var networkObject = other.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            CustomLogger.Log($"Object {other.name} does not have a NetworkObject component.", "BUILDING");
            return;
        }


        if (_playersToViewPreview == null)
        {
            CustomLogger.Log("_playersToViewPreview is null!", "BUILDING");
            return;
        }

        if (!_playersToViewPreview.Contains(networkObject.OwnerClientId))
        {
            _playersToViewPreview.Add(networkObject.OwnerClientId);
            CustomLogger.Log($"Added player {networkObject.OwnerClientId} to preview list.", "BUILDING");
        }

        if (_thisObjectNetworkId == 0)
        {
            CustomLogger.Log("_thisObjectNetworkId is uninitialized or 0!", "BUILDING");
            return;
        }

        // Convert list to array and send RPCs
        ulong[] playersArray = _playersToViewPreview.ToArray();

        ulong networkObjectId = GetComponent<NetworkObject>().NetworkObjectId;
        SendPreviewToClientsServerRpc(false, networkObjectId);

        SendUIToClientsServerRpc(networkObject.OwnerClientId, false);
        SetBuildingNetIdServerRpc(_thisObjectNetworkId, networkObject.OwnerClientId);
        SetCurrentLevelServerRpc(_currentLevel, networkObject.OwnerClientId);
    }


    private void OnTriggerExit(Collider other)
    {
        
        if (!IsServer) return; // Ensure this runs only on the server

        // Get the NetworkObject component
        NetworkObject networkObject = other.GetComponent<NetworkObject>();

        // Check if the networkObject exists and if it belongs to a player
        if (networkObject != null && networkObject.IsPlayerObject)
        {
            // Remove the player from the list
            ulong clientId = networkObject.OwnerClientId;
            _playersToViewPreview.Remove(clientId);

            // Inform the clients that the preview should be hidden
            ulong[] playersArray = _playersToViewPreview.ToArray();

            ulong networkObjectId = GetComponent<NetworkObject>().NetworkObjectId;
            SendPreviewToClientsServerRpc(false, networkObjectId);

            // Send UI updates to the exiting player (this will depend on your specific game logic)
            SendUIToClientsServerRpc(clientId, true);

            // Reset building-specific data for the exiting player
            SetBuildingNetIdServerRpc(0, clientId);
            SetCurrentLevelServerRpc(0, clientId);

            // Optional Debug Log to track changes
            CustomLogger.Log($"Player {clientId} exited the trigger. Preview hidden and UI updated.", "BUILDING");
        }
        else
        {
            CustomLogger.Log("OnTriggerExit: The object does not have a NetworkObject component or is not a player.", "BUILDING");
        }
    }

    #region OnTriggerHelperFunctions
    private void MoveUINearPlayer(ulong clientId)
    {
        if (!_staticUI)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var clientData))
            {
                NetworkObject playerNetworkObject = clientData.PlayerObject;
                Transform playerTransform = playerNetworkObject.gameObject.transform;
                Vector3 targetPosition = playerTransform.position + playerTransform.forward * distanceInFront;
                targetPosition.y = playerTransform.position.y + 1.5f; // Example height offset
                buildingUI.transform.position = targetPosition;
                buildingUI.transform.LookAt(playerTransform);
            }
        }
    }
    private void ResetUIOriginalPosition(){
        if(!_staticUI){
            buildingUI.transform.position = originalUIPosition;
        }
    }
    #endregion

    #region UI_STATES
    public bool IsMaxLevel()
    {
        // fix
        return _currentLevel < _buildingLevels.Length;
    }
    private bool IsInitLevel()
    {
        return _currentLevel == 0;
    }
    #endregion

    #region UPGRADE_FUNCTIONS

    private IEnumerator FillLoadingBar()
    {
        // Reset the loading bar
        loadingBar.gameObject.SetActive(true);
        loadingBar.value = 0;

        float elapsedTime = 0f;

        // Fill the bar over time
        while (elapsedTime < _buildTime)
        {
            elapsedTime += Time.deltaTime;

            // Update the loading bar value
            loadingBar.value = Mathf.Clamp01(elapsedTime / _buildTime);

            // Update the message text with the remaining time
            float remainingTime = _buildTime - elapsedTime;
            messageText.text = $"Upgrading: {(int)remainingTime} seconds";
            yield return null; // Wait until the next frame
        }

        // Ensure the bar is fully filled
        loadingBar.value = 1f;
        AudioManager.Instance.StopRepeatingSFX();
        // spawn the object and refresh next preview object if any currentLevel++
        SpawnObject();
        if (_currentLevel < _buildingLevels.Length)
        {
            _currentLevel++;
            SpawnPreviewObject();
        }
        UpdatePreviewForPlayers();
        _isBuilding = false;
    }

    private void SpawnPreviewObject()
    {
        if (_buildingLevels.Length > 0)
        {
            if (!IsInitLevel())
            {
                _currentPreviewModel.GetComponent<NetworkObject>().Despawn();
            }

            _currentPreviewModel = Instantiate(_buildingLevels[_currentLevel].previewModel, _buildingSpawnPoint[_currentLevel].position, _buildingSpawnPoint[_currentLevel].rotation);
            NetworkObject networkObject = _currentPreviewModel.GetComponent<NetworkObject>();
            networkObject.Spawn();

            // Remove any old models before adding a new one
            if (previewModelsByNetworkId.ContainsKey(networkObject.NetworkObjectId))
            {
                previewModelsByNetworkId[networkObject.NetworkObjectId] = _currentPreviewModel;
                Debug.Log($"Updated preview model with NetworkObjectId {networkObject.NetworkObjectId}.");
            }
            else
            {
                previewModelsByNetworkId.Add(networkObject.NetworkObjectId, _currentPreviewModel);
                Debug.Log($"Added new preview model with NetworkObjectId {networkObject.NetworkObjectId}.");
            }
        }
    }
    private void SpawnObject()
    {
        if (_buildingLevels[_currentLevel] != null)
        {
            if(_currentModel != null && !_overlapBuildings)
            {
                _currentModel.GetComponent<NetworkObject>().Despawn();
            }
            _currentModel = Instantiate(_buildingLevels[_currentLevel].buildingModel, _buildingSpawnPoint[_currentLevel].position, _buildingSpawnPoint[_currentLevel].rotation);
            _currentModel.GetComponent<NetworkObject>().Spawn(true);
        }
    }
    #endregion

    #region RPC
    [ServerRpc]
    private void SendPreviewToClientsServerRpc(bool toShow, ulong objNetworkId, ServerRpcParams rpcParams = default)
    {
        ulong[] clientsToNotify = _playersToViewPreview.ToArray();
        SendPreviewClientRpc(clientsToNotify, objNetworkId, toShow);
        Debug.Log($"Server sending preview visibility {toShow} for model {objNetworkId} to clients.");
    }

    [ClientRpc]
    private void SendPreviewClientRpc(ulong[] players, ulong objNetworkId, bool toShow)
    {
        if (players == null || players.Length == 0) return;

        foreach (ulong clientId in players)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                if (_currentPreviewModel != null)
                {
                    NetworkObject networkObject = _currentPreviewModel.GetComponent<NetworkObject>();
                    Debug.Log($"Trying to access model with NetworkObjectId {networkObject.NetworkObjectId}");

                    // Only try to access if the NetworkObjectId matches
                    if (networkObject.NetworkObjectId == objNetworkId)
                    {
                        _currentPreviewModel.SetActive(toShow);
                        Debug.Log($"Preview visibility set for NetworkObjectId {networkObject.NetworkObjectId}");
                    }
                    else
                    {
                        Debug.LogWarning($"NetworkObjectId mismatch: Expected {objNetworkId}, but found {networkObject.NetworkObjectId}");
                    }
                }
                else
                {
                    Debug.LogWarning("Current preview model is null.");
                }
            }
        }
    }
    [ServerRpc]
    private void SendUIToClientsServerRpc(ulong clientId, bool originalPosition, ServerRpcParams rpcParams = default)
    {
        SendUIPositionClientRpc(clientId, originalPosition);
    }
    [ClientRpc]
    private void SendUIPositionClientRpc(ulong clientId,bool originalPosition)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            if (originalPosition)
            {
                ResetUIOriginalPosition();
            }
            else
            {
                MoveUINearPlayer(clientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BeginBuildServerRpc(ServerRpcParams rpcParams = default){
        if (!IsMaxLevel() && !IsServer && _isBuilding) return;
        _isBuilding = true;
        AudioManager.Instance.PlayRepeatingSFX(buildingSFX);
        _buildTime = _buildingLevels[_currentLevel].buildTime;
        loadingCoroutine = StartCoroutine(FillLoadingBar());
    }
    // build state transfer
    [ServerRpc]
    public void RequestBuildingStateServerRpc(ServerRpcParams rpcParams = default)
    {
        // When the client calls this, the server responds by sending the bool value back
        RespondBuildingSateClientRpc(_isBuilding, rpcParams.Receive.SenderClientId);
    }

    // ClientRpc to send the response back to the client
    [ClientRpc]
    private void RespondBuildingSateClientRpc(bool value, ulong clientId)
    {
        // Check if this is the correct client (optional, useful in a multiplayer game)
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // set BuildingByssy in PlayerController
            PlayerController plr = GetPlayerByClientId(clientId);
            plr.BuildingBussy = value;
        }
    }
    // building network id
    [ServerRpc]
    public void SetBuildingNetIdServerRpc(ulong netId, ulong clientId,ServerRpcParams rpcParams = default)
    {
        // When the client calls this, the server responds by sending the bool value back
        SetBuildingNetIdClientRpc(netId, clientId);
    }
    [ClientRpc]
    private void SetBuildingNetIdClientRpc(ulong netId, ulong clientId)
    {
        // Check if this is the correct client (optional, useful in a multiplayer game)
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // set BuildingNetId in PlayerController
            PlayerController plr = GetPlayerByClientId(clientId);
            plr.BuildingNetId = netId;
        }
    }

    [ServerRpc]
    public void SetCurrentLevelServerRpc(int level, ulong clientId, ServerRpcParams rpcParams = default)
    {
        // When the client calls this, the server responds by sending the bool value back
        SetCurrentLevelClientRpc(level, clientId);
    }
    [ClientRpc]
    private void SetCurrentLevelClientRpc(int level, ulong clientId)
    {
        // Check if this is the correct client (optional, useful in a multiplayer game)
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // set BuildingNetId in PlayerController
            PlayerController plr = GetPlayerByClientId(clientId);
            plr.CurrentBuildingLevel = level;
        }
    }
    #endregion

    #region RPC_HELPERS

    private PlayerController GetPlayerByClientId(ulong clientId)
    {
        PlayerController player = null;
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var clientData))
        {
            // Get the PlayerObject associated with the client
            NetworkObject playerObject = clientData.PlayerObject;

            if (playerObject != null)
            {
                // Try to get the PlayerController component from the PlayerObject
                PlayerController playerController = playerObject.GetComponent<PlayerController>();

                if (playerController != null)
                {
                    // Successfully found the PlayerController
                    player = playerController;
                    //Debug.Log($"Found PlayerController for client {clientId}: {playerController.name}");
                }
            }
        }
        return player;
    }

    #endregion

    #region PUBLIC_ACCESS
       
    public int GetLevelsCount()
    {
        return _buildingLevels.Length;
    }
    public int GetLevelMaterialsCount(int level)
    {
        return _buildingLevels[level].reqMaterials.Length;
    }
    public BuildingLevelsSO.Materials[] GetLevelMaterials(int level)
    {
        return _buildingLevels[level].reqMaterials;
    }
    public float GetBuildingTime(int level)
    {
        return _buildingLevels[level].buildTime;
    }
    public int GetMaxLevel()
    {
        return _buildingLevels.Length;
    }
    #endregion

    #region VISUALIZATION_EDITOR
    private void OnDrawGizmos()
    {
        // Set the gizmo color to the user-defined color
        Gizmos.color = gizmoColor;

        // Create a Vector3 for the offset and apply it to the cube's position
        Vector3 gizmoOffset = new Vector3(offsetX, offsetY, offsetZ);

        // Draw a wireframe cube (outline only) at the object's position + offset
        Gizmos.DrawWireCube(transform.position + gizmoOffset, new Vector3(width, height, depth));

        // Draw text if a custom editor is available
        DrawGizmoText();
    }

    // Draw the text label with adjustable size and color
    private void DrawGizmoText()
    {
        // Only draw text in the scene if we are in the Editor
        #if UNITY_EDITOR
        UnityEditor.Handles.BeginGUI();

        // Set the color and size for the text
        GUIStyle style = new GUIStyle();
        style.normal.textColor = gizmoColor;
        style.fontSize = Mathf.RoundToInt(textSize);

        // Position the text slightly above the gizmo's position
        Vector3 textPosition = transform.position + new Vector3(offsetX, offsetY, offsetZ) + Vector3.up * height;

        // Draw the text at the specified position
        UnityEditor.Handles.Label(textPosition, buildingName, style);

        UnityEditor.Handles.EndGUI();
        #endif
    }

    internal string GetBuildingName()
    {
        return buildingName;
    }
    #endregion
}
