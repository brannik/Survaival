using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    [SerializeField] public List<ItemSO> ItemsDB;
    [SerializeField] public List<BuildingLevelsSO> BuildingLevels;

    public static ItemDatabase Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple instances of ItemDatabase found. Destroying the duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional if you want the database to persist across scenes.
    }

    public void AddItem(ItemSO item)
    {
        if (item != null && !ItemsDB.Contains(item))
        {
            ItemsDB.Add(item);
        }
    }

    public void RemoveItem(ItemSO item)
    {
        if (item != null && ItemsDB.Contains(item))
        {
            ItemsDB.Remove(item);
        }
    }
}
