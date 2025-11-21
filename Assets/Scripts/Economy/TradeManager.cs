using System.Collections.Generic;
using UnityEngine;

public static class TradeManager
{
    public static bool TradeItemToMarket(Party party, Town town, ItemData item, int quantity)
    {
        if (!LivingWorldRPG.Core.GameFeatureFlags.TradingEnabled)
        {
            Debug.LogWarning("Trading is disabled.");
            return false;
        }
        return ResolveService()?.TradeItemToMarket(party, town, item, quantity) ?? false;
    }

    public static bool TradeItemFromMarket(Party party, Town town, ItemData item, int quantity)
    {
        if (!LivingWorldRPG.Core.GameFeatureFlags.TradingEnabled)
        {
            Debug.LogWarning("Trading is disabled.");
            return false;
        }
        return ResolveService()?.TradeItemFromMarket(party, town, item, quantity) ?? false;
    }

    private static ITradeService ResolveService()
    {
        if (_tradeService == null)
        {
            if (!ServiceLocator.TryResolve(out _tradeService))
            {
                _tradeService = TradeService.Instance;
            }
        }

        return _tradeService;
    }

    private static ITradeService _tradeService;
}
