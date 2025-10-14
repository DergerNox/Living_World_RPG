using UnityEngine;

[System.Serializable]
public class Item : MonoBehaviour
{
    public string name;
    public ItemType type;
    public int value;
    public int attackBonus;
    public int defenseBonus;
    public int healthBonus;
    public int manaBonus;
}

public enum ItemType { Consumable, Weapon, Armor, Accessory }
