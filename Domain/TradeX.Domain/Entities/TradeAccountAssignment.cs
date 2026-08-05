using TradeX.Domain.Abstractions.Entities;

namespace TradeX.Domain.Entities;

public partial class TradeAccountAssignment : BaseEntity
{
    public Guid TradeId { get; set; }
    public Guid TradingAccountId { get; set; }
}
