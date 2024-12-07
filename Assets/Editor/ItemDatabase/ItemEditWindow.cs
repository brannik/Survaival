using UnityEditor;
using UnityEngine;
using static ENUMS;

public class ItemEditWindow : EditorWindow
{
    private string itemAssetName;
    private ItemSO itemToEdit;
    private ItemDatabase itemDatabase;

    private string itemName;
    private ItemSubtype itemSubtype;
    private string itemDescription;
    private int itemMaxStack;
    private Sprite itemSprite;
    private PickupType itemPickupType;
    private Quality itemQuality;

    

    // The method to open the edit window
    public static void OpenWindow(ItemSO item, ItemDatabase database)
    {
        ItemEditWindow window = GetWindow<ItemEditWindow>("Edit Item");
        window.itemToEdit = item;
        window.itemDatabase = database;

        // Initialize fields with current values of the item
        window.itemName = item.itemName;
        window.itemSubtype = item.subType;
        window.itemDescription = item.itemDescription;
        window.itemMaxStack = item.maxStack;
        window.itemSprite = item.itemSprite;
        window.itemPickupType = item.pickupType;
        window.itemQuality = item.quality;

        // Set the asset name field to the current name of the asset
        window.itemAssetName = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(item));

        window.Show();
    }

    private void OnGUI()
    {
        if (itemToEdit == null)
        {
            EditorGUILayout.HelpBox("No item selected to edit.", MessageType.Warning);
            return;
        }
        GUIStyle centeredStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,  // Horizontally center the text
            fontStyle = FontStyle.Bold
        };

        // Set a fixed width for the label (optional, but it helps with horizontal centering)
        float labelWidth = EditorGUIUtility.currentViewWidth - 20;  // Adjust the width as needed

        EditorGUILayout.LabelField($"===[{itemToEdit.itemName}]===", centeredStyle, GUILayout.Width(labelWidth));
        EditorGUILayout.Space();


        // New field to set asset name separately
        itemAssetName = EditorGUILayout.TextField("Asset Name", itemAssetName);
        itemName = EditorGUILayout.TextField("Item Name", itemName);
        itemSubtype = (ItemSubtype)EditorGUILayout.EnumPopup("Item Subtype", itemSubtype);
        itemDescription = EditorGUILayout.TextArea(itemDescription, GUILayout.Height(80));
        itemMaxStack = EditorGUILayout.IntField("Max Stack", itemMaxStack);
        itemSprite = (Sprite)EditorGUILayout.ObjectField("Item Sprite", itemSprite, typeof(Sprite), false);
        itemPickupType = (PickupType)EditorGUILayout.EnumPopup("Pickup Type", itemPickupType);
        itemQuality = (Quality)EditorGUILayout.EnumPopup("Quality", itemQuality);

        

        if (GUILayout.Button("Save Changes"))
        {
            SaveItemChanges();
        }

        if (GUILayout.Button("Cancel"))
        {
            Close();  // Close the edit window without saving
        }
    }

    private void SaveItemChanges()
    {
        if (itemToEdit == null) return;

        // Update the item with the new values
        itemToEdit.itemName = itemName;
        itemToEdit.subType = itemSubtype;
        itemToEdit.itemDescription = itemDescription;
        itemToEdit.maxStack = itemMaxStack;
        itemToEdit.itemSprite = itemSprite;
        itemToEdit.pickupType = itemPickupType;
        itemToEdit.quality = itemQuality;

        // Rename the asset if the asset name has changed
        string assetPath = AssetDatabase.GetAssetPath(itemToEdit);
        string newAssetPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assetPath), itemAssetName + ".asset");

        if (assetPath != newAssetPath)
        {
            AssetDatabase.RenameAsset(assetPath, itemAssetName);
            AssetDatabase.SaveAssets();
            Debug.Log($"Asset renamed to '{itemAssetName}'.");
        }

        // Mark the database as dirty so Unity knows it has changed
        EditorUtility.SetDirty(itemToEdit);

        // Optionally, save the updated item to disk if necessary
        AssetDatabase.SaveAssets();

        Debug.Log($"Item '{itemName}' edited successfully.");

        // Close the window after saving changes
        Close();
    }
}
