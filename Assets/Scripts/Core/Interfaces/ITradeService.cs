/// <summary>
/// Provides methods for trading items between parties and settlement markets.
/// </summary>
public interface ITradeService
{
    bool TradeItemToMarket(Party party, Town town, ItemData item, int quantity);
    bool TradeItemFromMarket(Party party, Town town, ItemData item, int quantity);
}
