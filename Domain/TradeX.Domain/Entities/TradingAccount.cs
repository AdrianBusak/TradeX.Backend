using TradeX.Domain.Abstractions.Entities;
using TradeX.Domain.Enums;

namespace TradeX.Domain.Entities;

public partial class TradingAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public TradingAccountType AccountType { get; set; }
    public string Broker { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public decimal InitialBalance { get; set; }
    public decimal CurrentBalance { get; set; }
}
