using System.Drawing;

public class ENUMS
{
    public enum PickupType{
        Item,
        Money,
        Buff
    }
    public enum ItemSubtype{
        Food,
        Material,
        Weapon,
        Armor
    }

    public enum Quality{
        Poor = 0,
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        Epic = 4,
        Legendary = 5,
        Artifact = 6

    }
    public static string[] QualityColors = { "#9d9d9d", "#ffffff", "#1eff00", "#0070dd", "#a335ee","#ff8000","#e6cc80" };

}
