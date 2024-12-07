using Unity.Netcode;
using UnityEngine;

public class ExclusiveObject : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // This code will only run for the client that owns this object
            EnableClientExclusiveFeatures();
        }
        else
        {
            // Disable interaction for other clients
            DisableClientExclusiveFeatures();
        }
    }

    private void EnableClientExclusiveFeatures()
    {
        // Enable features for the owner client
        //Debug.Log("This object is owned by me!");
    }

    private void DisableClientExclusiveFeatures()
    {
        // Disable features for non-owners (e.g., disable components)
        //Debug.Log("This object is not owned by me!");
        // Example: Disable interaction scripts
        gameObject.SetActive(false);
    }
}
