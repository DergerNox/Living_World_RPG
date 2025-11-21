using System.Collections.Generic;
using UnityEngine;
using LivingWorldRPG.Core;

[System.Serializable]
public class Market
{
    public List<ItemData> availableItems = new List<ItemData>();
    public Dictionary<string, int> supply = new Dictionary<string, int>();
    public Dictionary<string, int> demand = new Dictionary<string, int>();
    public Dictionary<string, int> prices = new Dictionary<string, int>();

    public void UpdatePrices()
    {
        if (!GameFeatureFlags.TradingEnabled)
        {
            return;
        }
        foreach (var item in availableItems)
        {
            int basePrice = item.value;
            int sup = supply.ContainsKey(item.name) ? supply[item.name] : 0;
            int dem = demand.ContainsKey(item.name) ? demand[item.name] : 0;
            prices[item.name] = Mathf.Max(1, basePrice + (dem - sup) * 10); // simple formula, min 1
        }
    }

    public int GetPrice(ItemData item)
    {
        if (!GameFeatureFlags.TradingEnabled)
        {
            return item != null ? item.value : 0;
        }
        return prices.ContainsKey(item.name) ? prices[item.name] : item.value;
    }

    public void AddItem(ItemData item)
    {
        if (!GameFeatureFlags.TradingEnabled)
        {
            return;
        }
        if (!availableItems.Contains(item))
        {
            availableItems.Add(item);
            supply[item.name] = 0;
            demand[item.name] = 0;
            UpdatePrices();
        }
    }
}
