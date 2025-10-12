using System.Collections.Generic;
using UnityEngine;

public static class TradeManager
{
    // Trade items between a party and a town market
    public static bool TradeItemToMarket(Party party, Town town, Item item, int quantity)
    {
        if (party.TradeableItems.Contains(item) && party.Gold >= town.Market.GetPrice(item) * quantity)
        {
            party.Gold -= town.Market.GetPrice(item) * quantity;
            town.Market.supply[item.name] += quantity;
            party.TradeableItems.Remove(item);
            return true;
        }
        return false;
    }

    public static bool TradeItemFromMarket(Party party, Town town, Item item, int quantity)
    {
        if (town.Market.supply.ContainsKey(item.name) && town.Market.supply[item.name] >= quantity)
        {
            int cost = town.Market.GetPrice(item) * quantity;
            if (party.Gold >= cost)
            {
                party.Gold -= cost;
                town.Market.supply[item.name] -= quantity;
                party.TradeableItems.Add(item);
                return true;
            }
        }
        return false;
    }
}
