using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    [Range(5f, 20f)]
    public float spawnTimer = 10f;
    [SerializeField] public List<Transform> spawnLocations;
    [SerializeField] private List<ItemPickup> pickups;

    private HashSet<Transform> occupiedSlots = new HashSet<Transform>();

    void Start()
    {
        if (IsServer) // Only the server spawns items
        {
            InvokeRepeating("PerformOnlineSpawn", 0, spawnTimer);
        }
    }

    private void PerformOnlineSpawn()
    {
        // Check if there are any empty slots available
        int index = GetRandomEmptySlot();
        if (index == -1)
        {
            //Debug.LogWarning("No empty slots available for spawning.");
            return; // Stop spawning if all slots are occupied
        }

        // Instantiate the object and ensure no overlaps
        Transform spawnLocation = spawnLocations[index];
        int randomIndex = Random.Range(0, pickups.Count);
        GameObject spawnedObj = Instantiate(pickups[randomIndex].gameObject, spawnLocation.position, Quaternion.identity);

        // Attach to spawn point and mark it
        

        // Register this slot as occupied
        occupiedSlots.Add(spawnLocation);

        // Spawn the network object
        NetworkObject networkObject = spawnedObj.GetComponent<NetworkObject>();
        networkObject.Spawn(true);
        spawnedObj.transform.SetParent(spawnLocation);

        // Add despawn callback
        var gatherable = spawnedObj.GetComponent<Gatherable>();
        if (gatherable != null)
        {
            gatherable.OnDespawn += () =>
            {
                occupiedSlots.Remove(spawnLocation);
            };
        }
    }

    private int GetRandomEmptySlot()
    {
        // Create a randomized list of indices
        List<int> indices = new List<int>();
        for (int i = 0; i < spawnLocations.Count; i++)
        {
            indices.Add(i);
        }
        Shuffle(indices);

        // Find the first unoccupied slot
        foreach (int index in indices)
        {
            if (!occupiedSlots.Contains(spawnLocations[index]))
            {
                return index;
            }
        }

        // No empty slots available
        return -1;
    }

    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
