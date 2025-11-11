using System;

[Serializable]
public class ItemData
{
    public string name;
    public ItemType type;
    public int value;
    public int attackBonus;
    public int defenseBonus;
    public int healthBonus;
    public int manaBonus;

    public ItemData Clone()
    {
        return (ItemData)MemberwiseClone();
    }
}
