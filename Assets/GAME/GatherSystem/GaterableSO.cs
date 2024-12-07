using UnityEngine;
[CreateAssetMenu(menuName = "Game/Gathering System/New item")]
[System.Serializable]
public class GaterableSO : ScriptableObject
{
    public string objectName;
    public Vector2 dropAmount = new Vector2(1,3);
    public ItemSO lootItem;
    public float gatherTime = 2f;
    public Vector2 respawnTimer = new Vector2(5f,10f);

}
