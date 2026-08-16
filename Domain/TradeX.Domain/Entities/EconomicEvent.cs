using TradeX.Domain.Abstractions.Entities;
using TradeX.Domain.Enums;

namespace TradeX.Domain.Entities;

public sealed class EconomicEvent : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public DateTimeOffset ScheduledAt { get; set; }
    public EconomicImpact Impact { get; set; }
    public string? Forecast { get; set; }
    public string? Previous { get; set; }
    public DateTimeOffset LastSyncedAt { get; set; }
}
