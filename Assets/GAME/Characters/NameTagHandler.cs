using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Collections;
using System;

public class NameTagHandler : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    private NetworkVariable<FixedString64Bytes> nameTag = new NetworkVariable<FixedString64Bytes>("Null", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Subscribe to changes in NetworkVariables
        nameTag.OnValueChanged += OnNameTagChanged;


        if (IsOwner)
        {
            // Load saved colors from PlayerPrefs and request a change from the server
            LoadLocalName();
            RequestColorChangeServerRpc(nameTag.Value.ToString());
        }

        // Apply the colors (initially or after a change)
        ApplyName();
    }

    private void OnNameTagChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        ApplyName();
    }

    private void ApplyName()
    {
        playerNameText.text = nameTag.Value.ToString();
    }
    [ServerRpc(RequireOwnership = false)]
    private void RequestColorChangeServerRpc(string _name)
    {
        nameTag.Value = _name;
    }

    private void LoadLocalName()
    {
        // Check if the player is the owner (only the owner should load the preferences)
        if (IsOwner)
        {
            nameTag.Value = PlayerPrefs.GetString("char_name", "Player");
        }
    }
}