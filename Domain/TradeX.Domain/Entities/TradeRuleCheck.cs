using TradeX.Domain.Abstractions.Entities;

namespace TradeX.Domain.Entities;

public partial class TradeRuleCheck : BaseEntity
{
    public Guid TradeId { get; set; }

    public Guid StrategyRuleId { get; set; }

    public Guid UserId { get; set; }

    public bool IsFollowed { get; set; }

    public string? Note { get; set; }
}
