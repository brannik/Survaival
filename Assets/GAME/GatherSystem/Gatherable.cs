using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;

public class Gatherable : NetworkBehaviour
{
    [Header("Pickup")]
    [SerializeField] public GaterableSO gaterableSO;
    [SerializeField] private AudioClip SFX;
    private bool sfxHasPlayed = false;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI plantNameText;
    
    public bool theItemIsPicked = false;
    public float growDuration = 1f; // Time for the object to grow to full size
    private Vector3 originalScale;
    private ulong thisObjId;
    private int RandDrop;
    private List<ulong> interractingPlayersIds;

    void Awake(){
        //label.SetActive(false);
        plantNameText.text = gaterableSO.objectName;
        interractingPlayersIds = new List<ulong>();
    }
    void Update(){
        if (theItemIsPicked)
        {
            if(sfxHasPlayed == false)
            {
                AudioManager.Instance.PlaySFX(SFX);
                sfxHasPlayed= true;
            }

        }
        else
        {
            AudioManager.Instance.StopSFX();
        }
    }
    void Start(){

        originalScale = transform.localScale; // Store the original scale
        if (IsServer) // Ensure the server handles spawning
        {
            // Call the RPC to trigger the animation on all clients
            GrowUpAnimation();
        }else{
            RequestGrowAnimationServerRpc();
        }
        RandDrop = GetDropAmount(gaterableSO.dropAmount.x, gaterableSO.dropAmount.y);
    }

    public event Action OnDespawn;

    public void Despawn()
    {
        if (IsServer)
        {
            foreach (ulong clientId in interractingPlayersIds)
            {
                NotifyPlayerItemReceivedClientRpc(clientId, 0, 0, 0, 0);
            }
            OnDespawn?.Invoke();
            GetComponent<NetworkObject>().Despawn();
            Destroy(gameObject);
        }else{
            RequestDespawnServerRpc();
            
        }
    }
    
    #region FIX

    #endregion
    private void OnTriggerEnter(Collider other){
        if (other.GetComponent<NetworkObject>() != this && other.GetComponent<NetworkObject>().IsPlayerObject && theItemIsPicked == false)
        {
            //other.GetComponent<NetworkObject>().gameObject.GetComponent<PlayerController>().gaterabeSO = gaterableSO;
            

            if (IsServer)
            {

                interractingPlayersIds.Add(other.GetComponent<NetworkObject>().OwnerClientId);
                thisObjId = gameObject.GetComponent<NetworkObject>().NetworkObjectId;
                NotifyPlayerItemReceivedClientRpc(other.GetComponent<NetworkObject>().OwnerClientId, gaterableSO.lootItem.itemId, gaterableSO.gatherTime,RandDrop, thisObjId);
            }
            else
            {
                thisObjId = gameObject.GetComponent<NetworkObject>().NetworkObjectId;
                other.GetComponent<NetworkObject>().GetComponentInChildren<PlayerController>().ItemId = gaterableSO.lootItem.itemId;
                other.GetComponent<NetworkObject>().GetComponentInChildren<PlayerController>().ItemTimer = gaterableSO.gatherTime;
                other.GetComponent<NetworkObject>().GetComponentInChildren<PlayerController>().ItemAmount = RandDrop;
                other.GetComponent<NetworkObject>().GetComponentInChildren<PlayerController>().objId = thisObjId;
            }

        }
        
        
    }
    private void OnTriggerExit(Collider other){
        if (other.GetComponent<NetworkObject>() != this && other.GetComponent<NetworkObject>().IsPlayerObject && theItemIsPicked == false)
        {
            //other.GetComponent<NetworkObject>().gameObject.GetComponent<PlayerController>().gaterabeSO = gaterableSO;


            if (IsServer)
            {
                interractingPlayersIds.Remove(other.GetComponent<NetworkObject>().OwnerClientId);
                NotifyPlayerItemReceivedClientRpc(other.GetComponent<NetworkObject>().OwnerClientId, 0, 0,0,0);
            }
            else
            {
                other.GetComponent<NetworkObject>().GetComponentInChildren<PlayerController>().ItemId = 0;
                other.GetComponent<NetworkObject>().GetComponentInChildren<PlayerController>().ItemTimer = 0;
                other.GetComponent<NetworkObject>().GetComponentInChildren<PlayerController>().ItemAmount = 0;
                other.GetComponent<NetworkObject>().GetComponentInChildren<PlayerController>().objId = 0;
            }

        }
    }
    
    private bool CheckChance(float percentage)
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        return randomValue < percentage;
    }
    private int GetDropAmount(float min,float max)
    {
        return (int)UnityEngine.Random.Range(min,max);
    }

    #region SCALEUP

    // Using DOTween to scale the object smoothly
    private void GrowUpAnimation()
    {
        // Start with scale 0 and scale to original scale over time
        transform.localScale = Vector3.zero; // Start with scale 0
        transform.DOScale(originalScale, growDuration).SetEase(Ease.OutBounce); // Animate scaling over time
    }

    #endregion

    #region RPC

    [ServerRpc(RequireOwnership = false)]
    public void RequestDespawnServerRpc(ServerRpcParams rpcParams = default)
    {
        Despawn();
    }

    // RPC to request the scaling animation across all clients
    [ServerRpc(RequireOwnership = false)]
    private void RequestGrowAnimationServerRpc(ServerRpcParams rpcParams = default)
    {
        // Call a ClientRpc to perform the animation on all clients
        RequestGrowAnimationClientRpc();
    }


    // ClientRpc to trigger the scaling animation on all clients
    [ClientRpc]
    private void RequestGrowAnimationClientRpc()
    {
        GrowUpAnimation();
    }
    [ClientRpc]
    private void NotifyPlayerItemReceivedClientRpc(ulong clientId, int itemId,float _timer,int _amount,ulong _id)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            PlayerController playerController = FindAnyObjectByType<PlayerController>();
            playerController.ItemId = itemId;
            playerController.ItemTimer = _timer;
            playerController.ItemAmount = _amount;
            playerController.objId = _id;
            // Optionally: Update UI or provide feedback
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void FlagPickingUpServerRpc(bool state,ServerRpcParams rpcParams = default)
    {
        theItemIsPicked = state;
    }

    #endregion
}
