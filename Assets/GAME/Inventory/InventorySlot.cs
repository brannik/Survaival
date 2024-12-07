[System.Serializable]
public class InventorySlot 
{
    private int slotId;
    private ItemSO item;
    private int amount;
    private bool isEmpty;

    public InventorySlot(ItemSO _item,int _id){
        this.slotId = _id;
        this.item = _item;
        this.amount = 0;
        isEmpty = true;
    }
    public int GetId(){
        return this.slotId;
    }
    public ItemSO GetItem(){
        return this.item;
    }
    public void UpdateSlot(ItemSO item,int amount){
        this.item = item;
        this.amount = amount;
        isEmpty = false;
    }
    public int GetAmount(){
        return amount;
    }
    public bool IsEmptySlot(){
        return isEmpty;
    }
    public void SetAmount(int am){
        amount = am;
    }
    public void SetId(int _id){
        slotId = _id;
    }
}
