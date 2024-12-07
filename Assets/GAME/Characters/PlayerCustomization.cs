using Unity.Netcode;
using UnityEngine;

public class PlayerCustomization : NetworkBehaviour
{
    [SerializeField] private SO_Colors colors;   // So_Colors contains an array of color options
    [SerializeField] private GameObject underwearObj;
    [SerializeField] private GameObject hairObj;

    private Renderer glassesRenderer;
    private Renderer bodyRenderer;

    // NetworkVariables to sync color indices across clients
    private NetworkVariable<int> ubderwearColorIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> hairColorIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        if (underwearObj != null) glassesRenderer = underwearObj.GetComponent<Renderer>();
        if (hairObj != null) bodyRenderer = hairObj.GetComponent<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to changes in NetworkVariables
        ubderwearColorIndex.OnValueChanged += OnUnderwearColorChanged;
        hairColorIndex.OnValueChanged += OnHairColorChanged;

        if (IsOwner)
        {
            // Load saved colors from PlayerPrefs and request a change from the server
            LoadColors();
            RequestColorChangeServerRpc(ubderwearColorIndex.Value, hairColorIndex.Value);
        }

        // Apply the colors (initially or after a change)
        ApplyColors();
    }

    private void ApplyColors()
    {
        // Apply the colors to the player's glasses and body
        if (glassesRenderer != null && ubderwearColorIndex.Value < colors.colors.Length)
        {
            glassesRenderer.material.color = colors.colors[ubderwearColorIndex.Value];
        }

        if (bodyRenderer != null && hairColorIndex.Value < colors.colors.Length)
        {
            bodyRenderer.material.color = colors.colors[hairColorIndex.Value];
        }
    }

    // ServerRpc to request a color change from the client and update the server's NetworkVariable
    [ServerRpc(RequireOwnership = false)]
    private void RequestColorChangeServerRpc(int glassesIndex, int bodyIndex)
    {
        // The server updates the NetworkVariable values
        ubderwearColorIndex.Value = glassesIndex;
        hairColorIndex.Value = bodyIndex;
    }

    private void OnUnderwearColorChanged(int oldValue, int newValue)
    {
        // When the glasses color changes, apply it to the model
        ApplyColors();
    }

    private void OnHairColorChanged(int oldValue, int newValue)
    {
        // When the body color changes, apply it to the model
        ApplyColors();
    }

    private void LoadColors()
    {
        // Check if the player is the owner (only the owner should load the preferences)
        if (IsOwner)
        {
            // Load saved color preferences from PlayerPrefs
            ubderwearColorIndex.Value = PlayerPrefs.GetInt("underwear_color", 0);
            hairColorIndex.Value = PlayerPrefs.GetInt("hair_color", 0);
        }
    }
}
