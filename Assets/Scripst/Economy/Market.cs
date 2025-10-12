using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Market
{
    public List<Item> availableItems = new List<Item>();
    public Dictionary<string, int> supply = new Dictionary<string, int>();
    public Dictionary<string, int> demand = new Dictionary<string, int>();
    public Dictionary<string, int> prices = new Dictionary<string, int>();

    public void UpdatePrices()
    {
        foreach (var item in availableItems)
        {
            int basePrice = item.value;
            int sup = supply.ContainsKey(item.name) ? supply[item.name] : 0;
            int dem = demand.ContainsKey(item.name) ? demand[item.name] : 0;
            prices[item.name] = Mathf.Max(1, basePrice + (dem - sup) * 10); // simple formula, min 1
        }
    }

    public int GetPrice(Item item)
    {
        return prices.ContainsKey(item.name) ? prices[item.name] : item.value;
    }

    public void AddItem(Item item)
    {
        if (!availableItems.Contains(item))
        {
            availableItems.Add(item);
            supply[item.name] = 0;
            demand[item.name] = 0;
            UpdatePrices();
        }
    }
}
