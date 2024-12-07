using System.Collections;
using Unity.Netcode;
using DG.Tweening;
using UnityEngine;

public class ObjectGrow : NetworkBehaviour
{
    public float growDuration = 2f; // Time for the object to grow to full size
    public float shrinkDuration = 1f; // Time for the object to shrink to zero size before despawning

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale; // Store the original scale
        if (IsServer) // Ensure the server handles spawning
        {
            // Call the RPC to trigger the animation on all clients
            GrowUpAnimation();
        }else{
            RequestGrowAnimationServerRpc();
        }
    }

    // RPC to request the scaling animation across all clients
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

    // Using DOTween to scale the object smoothly
    private void GrowUpAnimation()
    {
        // Start with scale 0 and scale to original scale over time
        transform.localScale = Vector3.zero; // Start with scale 0
        transform.DOScale(originalScale, growDuration).SetEase(Ease.OutBounce); // Animate scaling over time
    }

    // Call this method to shrink the object before despawning
    public void ShrinkAndDespawn()
    {
        // Shrink the object smoothly over time
        transform.DOScale(Vector3.zero, shrinkDuration).SetEase(Ease.InBounce)
                 .OnComplete(() => DespawnObject());
    }

    // Despawn the object after shrinking
    private void DespawnObject()
    {
        if (IsServer)
        {
            // Optionally, you can despawn the object using network methods
            NetworkObject networkObject = GetComponent<NetworkObject>();
            networkObject.Despawn(); // Despawn from network
        }
        else
        {
            // For non-owner clients, just destroy the object
            Destroy(gameObject);
        }
    }
}
