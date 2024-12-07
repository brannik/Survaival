
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Building System/New Recipe")]
public class BuildingLevelsSO : ScriptableObject
{
    [System.Serializable]
    public struct Materials{
        public ItemSO item;
        public int reqAmount;
    }

    [SerializeField] public float buildTime = 10f;
    [SerializeField] public GameObject previewModel;
    [SerializeField] public GameObject buildingModel;
    [SerializeField] public Materials[] reqMaterials;
    
    
}
