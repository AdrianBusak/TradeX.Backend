using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.MachineLearning;

public sealed class PreTradeScoreRequest
{
    public Guid StrategyId { get; set; }
    public Guid TradingInstrumentId { get; set; }
    public TradeDirection Direction { get; set; }
    public TradingSession? Session { get; set; }
    public DateTime TradeDate { get; set; }
    public decimal? EntryPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public decimal? LotSize { get; set; }
    public decimal? RiskAmount { get; set; }
    public List<PreTradeRuleCheckRequest> RuleChecks { get; set; } = [];
}

public sealed class PreTradeRuleCheckRequest
{
    public Guid StrategyRuleId { get; set; }
    public bool IsFollowed { get; set; }
}

public sealed class PreTradeMlReadinessResponse
{
    public bool IsReady { get; set; }
    public int ClosedTradeCount { get; set; }
    public int PositiveCount { get; set; }
    public int NonPositiveCount { get; set; }
    public int MinimumRequired { get; set; }
    public int MinimumPositiveRequired { get; set; }
    public int MinimumNonPositiveRequired { get; set; }
    public bool HasActiveModel { get; set; }
    public string? Reason { get; set; }
}

public sealed class TrainPreTradeMlModelResponse
{
    public bool IsReady { get; set; }
    public string? ModelVersion { get; set; }
    public int SampleCount { get; set; }
    public int PositiveCount { get; set; }
    public int NonPositiveCount { get; set; }
    public DateTime? TrainedAt { get; set; }
    public string? Reason { get; set; }
}

public sealed class PreTradeScoreResponse
{
    public bool IsReady { get; set; }
    public decimal? PositiveOutcomeProbability { get; set; }
    public int? SampleCount { get; set; }
    public string? ModelVersion { get; set; }
    public DateTime? TrainedAt { get; set; }
    public string? Confidence { get; set; }
    public string Message { get; set; } = null!;
    public string Disclaimer { get; set; } =
        "Experimental estimate based on your previous trades. This is not financial advice.";
}
