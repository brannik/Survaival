using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerNameTagManager : NetworkBehaviour
{
    [SerializeField] private Canvas nameTagCanvas; // Reference to the canvas containing the name tag
    [SerializeField] private Camera mainCamera;     // Reference to the camera
    [SerializeField] private TextMeshProUGUI playerNameText;


    void Start()
    {
        // Check if this object is owned by the local player (host or client)
        if (IsOwner)
        {
            
            // Disable the name tag canvas for the local player
            if (nameTagCanvas != null)
            {
                nameTagCanvas.gameObject.SetActive(false);  // Hide the name tag for the local player
            }
        }
        else
        {
            // Enable the name tag canvas for other players
            if (nameTagCanvas != null)
            {
                nameTagCanvas.gameObject.SetActive(true);  // Show name tag for other players
            }

            // Optional: Always make the name tag face the camera for non-owners (billboarding)
            if (mainCamera != null)
            {
                nameTagCanvas.transform.LookAt(mainCamera.transform);
            }
        }

        // Make sure other players' name tags are visible for everyone
        SetNameTagVisibilityForOthers();
    }

    // Optional: Update method to make the name tag face the camera at runtime
    private void Update()
    {
        if (!IsOwner && mainCamera != null)
        {
            nameTagCanvas.transform.LookAt(mainCamera.transform);
        }
    }

    // This method ensures the correct visibility of name tags based on ownership
    private void SetNameTagVisibilityForOthers()
    {
        // If the current player is not the owner, ensure they see the other player's name tag
        if (!IsOwner)
        {
            // You can choose to enable name tag for the owner based on your specific logic
            // For instance, the owner can always see other people's name tags, etc.
        }
    }

    public void SetPlayerName(string name)
    {
        if (playerNameText != null)
        {
            playerNameText.text = name;
        }
    }

}
