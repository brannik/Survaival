using UnityEngine;
using Cinemachine;
using Unity.Netcode;

public class PlayerCameraController : NetworkBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Reference to the player's Cinemachine Virtual Camera
    public AudioListener listener;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            // Enable and prioritize the camera for the local player
            virtualCamera.Priority = 10;
            virtualCamera.gameObject.SetActive(true);
            listener.enabled = true;

            Debug.Log($"Activated camera for local player: {gameObject.name}");
        }
        else
        {
            // Deactivate and deprioritize for non-local players
            virtualCamera.Priority = 0;
            virtualCamera.gameObject.SetActive(false);
            listener.enabled = false;
            Debug.Log($"Deactivated camera for non-local player: {gameObject.name}");
        }
    }
}
