using TradeX.Domain.Abstractions.Entities;
using TradeX.Domain.Enums;

namespace TradeX.Domain.Entities;

public partial class TradingInstrument : BaseEntity
{
    public Guid UserId { get; set; }
    public string Symbol { get; set; } = null!;
    public MarketType MarketType { get; set; }
}
