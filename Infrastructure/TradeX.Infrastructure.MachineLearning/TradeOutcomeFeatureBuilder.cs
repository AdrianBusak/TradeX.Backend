using TradeX.Application.Clients.Features.MachineLearning;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;
using TradeX.Infrastructure.MachineLearning.Models;

namespace TradeX.Infrastructure.MachineLearning;

internal sealed class TradeOutcomeFeatureBuilder
{
    public TradeOutcomeTrainingRow CreateFromTrade(
        Guid strategyId,
        string symbol,
        MarketType marketType,
        TradeDirection direction,
        TradingSession? session,
        DateTime tradeDate,
        decimal? stopLoss,
        decimal? takeProfit,
        decimal? riskAmount,
        int ruleCheckCount,
        int followedRuleCheckCount,
        bool label)
    {
        return new TradeOutcomeTrainingRow
        {
            Label = label,
            StrategyId = strategyId.ToString(),
            Symbol = symbol,
            MarketType = marketType.ToString(),
            Direction = direction.ToString(),
            Session = session?.ToString() ?? "Unknown",
            DayOfWeek = (float)tradeDate.DayOfWeek,
            Hour = tradeDate.Hour,
            PlannedRiskReward = !stopLoss.HasValue || stopLoss.Value <= 0 || !takeProfit.HasValue
                ? 0
                : (float)(takeProfit.Value / stopLoss.Value),
            StopLossDistance = (float)(stopLoss ?? 0),
            RuleCheckCount = ruleCheckCount,
            FollowedRuleCheckCount = followedRuleCheckCount,
            RuleCompliancePercent = ruleCheckCount == 0
                ? 0
                : (float)followedRuleCheckCount / ruleCheckCount * 100,
            RiskAmount = (float)(riskAmount ?? 0)
        };
    }

    public TradeOutcomeTrainingRow CreateFromScoreRequest(
        PreTradeScoreRequest request,
        TradingInstrument instrument)
    {
        return CreateFromTrade(
            request.StrategyId,
            instrument.Symbol,
            instrument.MarketType,
            request.Direction,
            request.Session,
            request.TradeDate,
            request.StopLoss,
            request.TakeProfit,
            request.RiskAmount,
            request.RuleChecks.Count,
            request.RuleChecks.Count(ruleCheck => ruleCheck.IsFollowed),
            label: false);
    }

}
