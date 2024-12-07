using UnityEditor;
using UnityEngine;
using static ENUMS;

public class NewItemCreationWindow : EditorWindow
{
    private ItemDatabase itemDatabase;
    private string newItemAssetName = "NewItemAsset";
    // Fields for new item creation
    private string newItemName = "New Item";
    private ItemSubtype newItemSubtype;
    private string newItemDescription = "Description";
    private int newItemMaxStack = 1;
    private Sprite newItemSprite;
    private PickupType newItemPickupType;
    private Quality newItemQuality;

    private string objectDirectory;




    public static void OpenWindow(ItemDatabase database,string direct)
    {
        NewItemCreationWindow window = GetWindow<NewItemCreationWindow>("Create New Item");
        window.itemDatabase = database;
        window.objectDirectory = direct;
        window.Show();
    }

    private void OnGUI()
    {
        if (itemDatabase == null)
        {
            EditorGUILayout.HelpBox("ItemDatabase is not assigned.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("New Item Creation", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // New field to set asset name separately
        newItemAssetName = EditorGUILayout.TextField("Asset Name", newItemAssetName);
        // Fields for new item creation
        newItemName = EditorGUILayout.TextField("Item Name", newItemName);
        newItemSubtype = (ItemSubtype)EditorGUILayout.EnumPopup("Item Subtype", newItemSubtype);
        newItemDescription = EditorGUILayout.TextArea(newItemDescription, GUILayout.Height(80)); // Make the description field larger
        newItemMaxStack = EditorGUILayout.IntField("Max Stack", newItemMaxStack);
        newItemSprite = (Sprite)EditorGUILayout.ObjectField("Item Sprite", newItemSprite, typeof(Sprite), false);
        newItemPickupType = (PickupType)EditorGUILayout.EnumPopup("Pickup Type", newItemPickupType);
        newItemQuality = (Quality)EditorGUILayout.EnumPopup("Quality", newItemQuality);

        

        if (GUILayout.Button("Create Item"))
        {
            CreateAndAddNewItem();
        }

        if (GUILayout.Button("Cancel"))
        {
            Close();  // Close the window without saving
        }
    }

    private void CreateAndAddNewItem()
    {
        if (itemDatabase == null) return;

        // Create a new ItemSO
        ItemSO newItem = ScriptableObject.CreateInstance<ItemSO>();
        newItem.itemId = itemDatabase.ItemsDB.Count; // Automatically set the ID
        newItem.itemName = newItemName;
        newItem.subType = newItemSubtype;
        newItem.itemDescription = newItemDescription;
        newItem.maxStack = newItemMaxStack;
        newItem.itemSprite = newItemSprite;
        newItem.pickupType = newItemPickupType;
        newItem.quality = newItemQuality;

        // Use the new asset name specified by the user
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{objectDirectory}/{newItemAssetName}.asset");

        // Create and save the new item asset
        AssetDatabase.CreateAsset(newItem, assetPath);
        AssetDatabase.SaveAssets();

        // Add the new item to the item database
        itemDatabase.AddItem(newItem);
        EditorUtility.SetDirty(itemDatabase);
        Debug.Log($"New Item added to the database at {assetPath}");

        // Clear the fields after creating the item
        ClearFields();

        Close();  // Close the creation window after item is created
    }

    private void ClearFields()
    {
        newItemName = "New Item";  // Reset to default name
        newItemSubtype = 0;  // Reset to default enum value
        newItemDescription = "Description";  // Reset to default description
        newItemMaxStack = 1;  // Reset to default stack size
        newItemSprite = null;  // Clear sprite field
        newItemPickupType = 0;  // Reset to default enum value
        newItemQuality = 0;  // Reset to default enum value
        newItemAssetName = "NewItemAsset";  // Reset to default asset name
    }
}
