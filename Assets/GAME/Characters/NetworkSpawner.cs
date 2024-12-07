using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class NetworkSpawner : NetworkBehaviour
{
    [Header("Spawn Point Settings")]
    [SerializeField, Range(1f, 50f)] private float spawnRange = 10f; // Range for spawn points
    [SerializeField] private GameObject playerPrefab; // The player prefab to spawn
    public List<Transform> spawnPoints = new List<Transform>(); // List of spawn points (now public)

    #region EDITOR_VISUAL
    private void OnDrawGizmos()
    {
        // Draw spawn range as a wire circle
        Gizmos.color = Color.yellow; // Color for the range visualization
        DrawCircle(transform.position, spawnRange, 30); // Draw a circle at the GameObject's position with the spawn range

        // Draw spawn points in the editor
        Gizmos.color = Color.green;
        foreach (Transform spawnPoint in spawnPoints)
        {
            Gizmos.DrawSphere(spawnPoint.position, 0.5f);
        }
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        Vector3 previousPoint = center + new Vector3(radius, 0, 0);
        float angleStep = 360f / segments;

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius, 0, Mathf.Sin(angle * Mathf.Deg2Rad) * radius);
            Gizmos.DrawLine(previousPoint, newPoint);
            previousPoint = newPoint;
        }
    }

    
    #endregion

    public override void OnNetworkSpawn()
    {
        /*
        if (IsServer)
        {
            // Spawn the player for the host
            SpawnPlayers(OwnerClientId);
        }
        else
        {
            // Request the server to spawn this client's player
        RequestSpawnServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        */
        RequestSpawnServerRpc();
    }
    private void SpawnPlayers(ulong _clientId)
    {
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint != null)
        {
            // Spawn the player prefab at the spawn point
            GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

            // Add a Billboard component to the Nameplate if it doesn't already exist
            Transform nameplate = playerInstance.transform.Find("Nameplate");
            if (nameplate != null && nameplate.GetComponent<Billboard>() == null)
            {
                nameplate.gameObject.AddComponent<Billboard>();
            }

            var playerNetworkObject = playerInstance.GetComponent<NetworkObject>();

            // Spawn the player object on all clients
            playerNetworkObject.SpawnAsPlayerObject(_clientId);
        }
        else
        {
            Debug.LogWarning("No spawn points available!");
        }
    }


    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0) return null;

        // Get a random index from the spawn points list
        int randomIndex = Random.Range(0, spawnPoints.Count);
        return spawnPoints[randomIndex];
    }

    public void GenerateSpawnPoint()
    {
        // Generate a new spawn point within the specified range relative to the spawner's position
        Vector3 randomPosition = new Vector3(
            transform.position.x + Random.Range(-spawnRange, spawnRange), // Random X within range
            4f, // Set the height to +4 on the Y-axis
            transform.position.z + Random.Range(-spawnRange, spawnRange) // Random Z within range
        );

        // Check if the generated position is within the defined range
        if (Vector3.Distance(transform.position, randomPosition) <= spawnRange)
        {
            // Create a new GameObject for the spawn point
            GameObject newSpawnPoint = new GameObject("SpawnPoint");
            newSpawnPoint.transform.position = randomPosition;

            // Set the spawn point as a child of the GameObject this script is attached to
            newSpawnPoint.transform.parent = transform; // Set parent to this GameObject

            // Add the new spawn point to the list
            spawnPoints.Add(newSpawnPoint.transform);
        }
    }
    #region RPC
    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnServerRpc( ServerRpcParams rpcParams = default)
    {
        // Get the ID of the client making the request
        ulong requestingClientId = rpcParams.Receive.SenderClientId;
        // Spawn the player for the requesting client
        SpawnPlayers(requestingClientId);
    }
    #endregion 
}