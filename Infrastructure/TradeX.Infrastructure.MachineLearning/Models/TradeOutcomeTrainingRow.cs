namespace TradeX.Infrastructure.MachineLearning.Models;

internal sealed class TradeOutcomeTrainingRow
{
    public bool Label { get; set; }
    public string StrategyId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string MarketType { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Session { get; set; } = string.Empty;
    public float DayOfWeek { get; set; }
    public float Hour { get; set; }
    public float PlannedRiskReward { get; set; }
    public float StopLossDistance { get; set; }
    public float RuleCheckCount { get; set; }
    public float FollowedRuleCheckCount { get; set; }
    public float RuleCompliancePercent { get; set; }
    public float RiskAmount { get; set; }
}
