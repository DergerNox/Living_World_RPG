using System.Collections.Generic;

[System.Serializable]
public class Equipment
{
    public List<EquippedItem> equippedItems = new List<EquippedItem>();

    public void EquipItem(EquipmentSlot slot, Item item)
    {
        EquippedItem existing = equippedItems.Find(e => e.slot == slot);
        if (existing != null)
        {
            existing.item = item;
        }
        else
        {
            equippedItems.Add(new EquippedItem { slot = slot, item = item });
        }
    }

    public Item UnequipItem(EquipmentSlot slot)
    {
        EquippedItem existing = equippedItems.Find(e => e.slot == slot);
        if (existing != null)
        {
            Item item = existing.item;
            equippedItems.Remove(existing);
            return item;
        }
        return null;
    }

    public Item GetEquippedItem(EquipmentSlot slot)
    {
        EquippedItem existing = equippedItems.Find(e => e.slot == slot);
        return existing != null ? existing.item : null;
    }
}

[System.Serializable]
public class EquippedItem
{
    public EquipmentSlot slot;
    public Item item;
}

public enum EquipmentSlot { Head, Body, Weapon, Shield, Accessory }
