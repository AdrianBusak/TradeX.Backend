using TradeX.Domain.Abstractions.Entities;
using TradeX.Domain.Enums;

namespace TradeX.Domain.Entities;

public partial class StrategyRule : BaseEntity
{
    public Guid StrategyId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int Order { get; set; }

    public bool IsRequired { get; set; }

    public StrategyRuleCategory Category { get; set; } = StrategyRuleCategory.Entry;

    public StrategyRuleImportance Importance { get; set; } = StrategyRuleImportance.Medium;
}