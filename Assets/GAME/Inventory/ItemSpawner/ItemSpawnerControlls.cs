using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class ItemSpawnerControls : MonoBehaviour
{
    [Range(1f, 20f)]
    public float spawnRange = 10f;

    public GameObject spawnPointPrefab;
    public ItemSpawner otherScript;

    private const int circleSegments = 36;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 center = transform.position;
        float angleStep = 360f / circleSegments;
        
        for (int i = 0; i < circleSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 start = new Vector3(center.x + Mathf.Cos(angle) * spawnRange, center.y, center.z + Mathf.Sin(angle) * spawnRange);
            Vector3 end = new Vector3(center.x + Mathf.Cos(angle + angleStep * Mathf.Deg2Rad) * spawnRange, center.y, center.z + Mathf.Sin(angle + angleStep * Mathf.Deg2Rad) * spawnRange);
            Gizmos.DrawLine(start, end);
        }
    }

    public void CreateRandomSpawnPoint()
    {
        if (spawnPointPrefab != null && otherScript != null)
        {
            // Calculate a random position within the spawn range
            Vector3 randomPosition = transform.position + new Vector3(
                Random.Range(-spawnRange, spawnRange),
                0f,
                Random.Range(-spawnRange, spawnRange)
            );

            // Instantiate the spawn point at the random position
            GameObject newSpawnPoint = Instantiate(spawnPointPrefab, randomPosition, Quaternion.identity);

            // Set the parent to the spawner
            newSpawnPoint.transform.SetParent(transform);

            // Disable the NetworkObject temporarily to avoid GlobalObjectIdHash conflict
            NetworkObject networkObject = newSpawnPoint.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.gameObject.SetActive(false); // Disable the object to reset network state

                // Enable it again to ensure it gets a new GlobalObjectIdHash
                networkObject.gameObject.SetActive(true);

            }

            // Add the new spawn point to the spawnLocations list in the other script
            otherScript.spawnLocations.Add(newSpawnPoint.transform);
        }
        else
        {
            Debug.LogError("No Spawn Point Prefab assigned or other script reference missing!");
        }
    }

    private bool IsServer => NetworkManager.Singleton.IsServer; // Helper for checking if this is the server
}
