using TradeX.Domain.Abstractions.Entities;
using TradeX.Domain.Enums;

namespace TradeX.Domain.Entities;

public partial class Strategy : BaseEntity
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public MarketType MarketType { get; set; }

    public string? Color { get; set; }
}
