using System;
using System.Collections.Generic;

[Serializable]
public class EquipmentData
{
    public List<EquippedItemData> equippedItems = new List<EquippedItemData>();

    public void EquipItem(EquipmentSlot slot, ItemData item)
    {
        if (item == null)
        {
            return;
        }

        var existing = equippedItems.Find(e => e.slot == slot);
        if (existing != null)
        {
            existing.item = item;
        }
        else
        {
            equippedItems.Add(new EquippedItemData { slot = slot, item = item });
        }
    }

    public ItemData UnequipItem(EquipmentSlot slot)
    {
        var existing = equippedItems.Find(e => e.slot == slot);
        if (existing == null)
        {
            return null;
        }

        var item = existing.item;
        equippedItems.Remove(existing);
        return item;
    }

    public ItemData GetEquippedItem(EquipmentSlot slot)
    {
        return equippedItems.Find(e => e.slot == slot)?.item;
    }
}

[Serializable]
public class EquippedItemData
{
    public EquipmentSlot slot;
    public ItemData item;
}
