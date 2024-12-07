using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

// NOT UPDATING PROPERLY
// STARTS OPENNED
// TRY CUSTOM EVENS WHEN _buildingId IS CHANGED
public class BuildingUI : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI nextLevelIndicator;
    [SerializeField] private GameObject recipeRowPrefab;
    [SerializeField] private Transform recipeContent;
    [SerializeField] private TextMeshProUGUI buildTimeText;
    [SerializeField] private GameObject workingUI;
    [SerializeField] private GameObject maxLevelUI;
    [SerializeField] private GameObject noBuildingNearbyUI;
    [SerializeField] private GameObject buildingUI;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI buildingTimerText;

    private ulong _buildingId;
    private bool _isBussy;
    private int _level;
    private float buildTime;
    private bool canBuildThtRecipe = false;
    [SerializeField] private Inventory inventory;

    void Awake(){
        maxLevelUI.SetActive(false);
        workingUI.SetActive(false);
        noBuildingNearbyUI.SetActive(false);
        buildingUI.SetActive(false);
    }
    void Update(){
        UpdateUI();
    }
    private void OnEnable()
    {

        buildingUI.SetActive(false);
    }
    private void UpdateUI()
    {
        if(_buildingId == 0)
        {
            noBuildingNearbyUI.SetActive(true);
        }
        else
        {
            noBuildingNearbyUI.SetActive(false);
        }
        workingUI.SetActive(_isBussy);
        maxLevelUI.SetActive(false); // fix
        buildingUI.SetActive(!_isBussy);
    }

    public void InitSystem(ulong buildingNetId,bool bussy,int level){
        _buildingId = buildingNetId;
        _isBussy = bussy;
        _level = level;
        //title.text = FindObjectByNetId(_buildingId).GetComponent<BuildingObject>().GetBuildingName();
        title.text = "FIX obj find";
        LoadRecipe();
        GetBuildTime();
        buildingTimerText.text = $"Time: {buildTime}";
    }
    public void TestButton(){
        if(!CanBuild()) return;
        RemoveItemsFromInventory();
        NetworkObject obj = FindObjectByNetId(_buildingId);
        obj.gameObject.GetComponent<BuildingObject>().BeginBuildServerRpc();
        
        // start building
    }
    public void LoadRecipe(){
        foreach(Transform child in recipeContent){
            Destroy(child.gameObject);
        }
        if(_buildingId != 0)
        {
            BuildingObject obj = FindObjectByNetId(_buildingId).GetComponent<BuildingObject>();
            if (obj.GetLevelsCount() == 0 || obj.GetLevelsCount() == _level || obj.GetLevelMaterialsCount(_level) == 0)
            {
                // MAX LEVEL
                maxLevelUI.SetActive(false); // fix
            }
            else
            {
                BuildingLevelsSO.Materials[] materials = obj.GetLevelMaterials(_level);
                for (int i = 0; i < materials.Length; i++)
                {
                    var a = Instantiate(recipeRowPrefab, recipeContent);
                    // check items in inventory
                    bool checkItemCount = FindItemsInInventory(materials[i].item, materials[i].reqAmount);
                    a.GetComponent<RecipeRowElement>().InitData(materials[i].item, materials[i].reqAmount, checkItemCount);
                }
                GetBuildTime();
            }
        }
        
    }

    private void GetBuildTime(){
        BuildingObject obj = FindObjectByNetId(_buildingId).gameObject.GetComponent<BuildingObject>();
        int levelsCount = obj.GetLevelsCount();
        if (levelsCount == 0 && levelsCount == _level){
            buildTimeText.text = "Max level";
            nextLevelIndicator.text = "Max level";
            maxLevelUI.SetActive(true);
        }else if(_level < levelsCount){
            buildTime = obj.GetBuildingTime(_level);
            buildTimeText.text = $"Build time: {buildTime} s";
            nextLevelIndicator.text = $"Upgrade to {_level}";
        }   
    }

    private bool FindItemsInInventory(ItemSO item, int amount)
    {
        if(!inventory && inventory == null){
            //print("No inventory");
            canBuildThtRecipe = false;
            return false;
        }else{
            //print("inventory found !");
            
            int amnt =  inventory.GetItemAmountFromInventory(item);
            if(amnt >= amount){
                canBuildThtRecipe = true;
                return true;
            }else{
                canBuildThtRecipe = false;
                return false;
            }
        }
    }

    private void RemoveItemsFromInventory(){
        if(!inventory && inventory == null){
            //print("No inventory");
            canBuildThtRecipe = false;
        }else{
            //print("inventory found !");
            BuildingObject obj = FindObjectByNetId(_buildingId).gameObject.GetComponent<BuildingObject>();
            BuildingLevelsSO.Materials[] materials = obj.GetLevelMaterials(_level);
            for(int i=0;i<materials.Length;i++){
                inventory.RemoveItems(materials[i].item,materials[i].reqAmount);
            }
            LoadRecipe();
        }
        
    }
    private bool CanBuild(){
        return canBuildThtRecipe;
    }
    public void HideUI(){
        buildingUI.SetActive(false);
    }

    private NetworkObject FindObjectByNetId(ulong? netId)
    {
        if ( netId == null ) return null;
        NetworkObject obj = null;
        foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            // Compare the network ID (NetworkObjectId) with the one you're looking for
            if (networkObject.Key == netId)
            {
                // Found the NetworkObject
                NetworkObject foundNetworkObject = networkObject.Value;
                obj = foundNetworkObject;
                break;
            }
        }

        return obj;
    }
}
