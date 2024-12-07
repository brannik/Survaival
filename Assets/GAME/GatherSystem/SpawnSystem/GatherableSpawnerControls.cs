using UnityEngine;
using System.Collections.Generic;

public class GatherableSpawnerControls : MonoBehaviour
{
    [Range(1f, 20f)]
    public float spawnRange = 10f;

    public GameObject spawnPointPrefab;  // This can be an empty GameObject or any prefab you use to mark spawn points
    public GatherableSpawner otherScript;  // Reference to the GatherableSpawner that holds the spawn locations

    public int numberOfSpawnPoints = 5;  // Number of spawn points to create

    private const int circleSegments = 36;
    private const float minDistanceBetweenPoints = 2f;  // Minimum distance to prevent overlap

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

    // This function creates a random spawn point in the scene when called from the Editor
    public void CreateRandomSpawnPoint()
    {
        if (spawnPointPrefab != null && otherScript != null)
        {
            // Try creating a valid spawn point within the range and not too close to other spawn points
            Vector3 randomPosition = GetValidRandomPosition();
            if (randomPosition != Vector3.zero)
            {
                // Instantiate the spawn point at the random position
                GameObject newSpawnPoint = Instantiate(spawnPointPrefab, randomPosition, Quaternion.identity);

                // Set the parent to the spawner
                newSpawnPoint.transform.SetParent(transform);

                // Add the new spawn point to the spawnLocations list in the other script
                otherScript.spawnLocations.Add(newSpawnPoint.transform);
            }
            else
            {
                Debug.LogError("Could not find a valid position for a new spawn point!");
            }
        }
        else
        {
            Debug.LogError("No Spawn Point Prefab assigned or other script reference missing!");
        }
    }

    // This function creates multiple spawn points in the scene when called from the Editor
    public void CreateMultipleSpawnPoints()
    {
        for (int i = 0; i < numberOfSpawnPoints; i++)
        {
            CreateRandomSpawnPoint();
        }
    }

    // This function removes all the spawn points created by this script
    public void RemoveAllSpawnPoints()
    {
        // Loop through all child transforms and remove them
        foreach (Transform spawnPoint in transform)
        {
            DestroyImmediate(spawnPoint.gameObject);  // Destroy in the Editor immediately
        }

        // Clear the list of spawn locations in the GatherableSpawner script
        if (otherScript != null)
        {
            otherScript.spawnLocations.Clear();
        }
    }

    private Vector3 GetValidRandomPosition()
    {
        Vector3 randomPosition = transform.position + new Vector3(
            Random.Range(-spawnRange, spawnRange),
            0f,
            Random.Range(-spawnRange, spawnRange)
        );

        // Check if the new position is too close to any existing spawn point
        foreach (Transform spawnLocation in otherScript.spawnLocations)
        {
            if (Vector3.Distance(randomPosition, spawnLocation.position) < minDistanceBetweenPoints)
            {
                return Vector3.zero; // Return invalid position if too close to another point
            }
        }

        return randomPosition; // Return valid position
    }
}
