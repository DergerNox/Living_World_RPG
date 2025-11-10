using UnityEngine;

/// <summary>
/// Default trade service implementing buy/sell logic between parties and towns.
/// </summary>
public class TradeService : MonoBehaviour, ITradeService
{
    public static TradeService Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public bool TradeItemToMarket(Party party, Town town, Item item, int quantity)
    {
        if (party == null || town == null || item == null || quantity <= 0) return false;
        var market = town.Market;
        if (market == null) return false;

        int price = market.GetPrice(item);
        int totalCost = price * quantity;
        if (!party.TradeableItems.Contains(item) || party.Gold < totalCost) return false;

        party.Gold -= totalCost;
        if (!market.supply.ContainsKey(item.name))
        {
            market.supply[item.name] = 0;
        }
        market.supply[item.name] += quantity;
        party.TradeableItems.Remove(item);
        return true;
    }

    public bool TradeItemFromMarket(Party party, Town town, Item item, int quantity)
    {
        if (party == null || town == null || item == null || quantity <= 0) return false;
        var market = town.Market;
        if (market == null) return false;

        if (!market.supply.ContainsKey(item.name) || market.supply[item.name] < quantity) return false;

        int totalCost = market.GetPrice(item) * quantity;
        if (party.Gold < totalCost) return false;

        party.Gold -= totalCost;
        market.supply[item.name] -= quantity;
        party.TradeableItems.Add(item);
        return true;
    }
}
