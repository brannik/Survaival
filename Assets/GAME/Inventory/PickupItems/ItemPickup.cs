using System;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ItemPickup : NetworkBehaviour
{
    [SerializeField] public Image itemSprite;
    [SerializeField] public TextMeshProUGUI itemName;
    [SerializeField] public TextMeshProUGUI amountVisual;
    [SerializeField] public ItemSO item;
    [SerializeField] public int amount;

    public override void OnNetworkDespawn()
    {
        Destroy(gameObject);
    }

    private void Awake()
    {
        UpdateVisuals();
    }

    private void OnValidate()
    {
        // Update visuals in the editor when itemSO or amount is changed
        if (item != null)
        {
            UpdateVisuals();
        }
    }

    public void UpdateVisuals()
    {
        if (item != null)
        {
            // Update sprite, name, and amount visual
            if (itemSprite != null) itemSprite.sprite = item.itemSprite;
            if (itemName != null) itemName.text = item.itemName;
            if (amountVisual != null) amountVisual.text = amount.ToString();
        }
    }
    public event Action OnDespawn;

    public void Despawn()
    {
        if (IsServer)
        {
            OnDespawn?.Invoke();
            GetComponent<NetworkObject>().Despawn();
            Destroy(gameObject);
        }else{
            RequestDespawnServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestDespawnServerRpc()
    {
        // The server will handle the despawning
        Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is a player
        if (!other.CompareTag("Player")) return;

        // Get the player's inventory
        var playerController = other.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("No PlayerController found on the player object.");
            return;
        }

        Inventory playerInventory = playerController.inventory;

        if (playerInventory != null && playerInventory.AddItem(item, amount))
        {
            Despawn();
        }
    }

    // ServerRpc to handle despawning the object on the server
    [ServerRpc(RequireOwnership = false)]
    private void RequestDespawnServerRpc(ulong objectId, ServerRpcParams rpcParams = default)
    {
        // Verify the object exists before despawning
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var networkObject))
        {
            networkObject.Despawn();
        }
    }
}

#if UNITY_EDITOR


[CustomEditor(typeof(ItemPickup))]
public class ItemPickupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target script
        ItemPickup itemPickup = (ItemPickup)target;

        // Draw the default inspector properties
        DrawDefaultInspector();

        // If itemSO is assigned, update the visuals manually (in case it's changed)
        if (itemPickup.item != null)
        {
            itemPickup.UpdateVisuals();
        }

        // Ensure changes are saved in the editor
        if (GUI.changed)
        {
            EditorUtility.SetDirty(itemPickup);
        }
    }
}
#endif
