using TradeX.Application.Abstractions.Interfaces;
using TradeX.Domain.Enums;
using TradeX.Infrastructure.MachineLearning.Models;

namespace TradeX.Infrastructure.MachineLearning;

internal sealed class TradeOutcomeDatasetBuilder(
    ITradeXRepository repository,
    TradeOutcomeFeatureBuilder featureBuilder)
    : ITradeOutcomeDatasetBuilder
{
    public async Task<List<TradeOutcomeTrainingRow>> BuildAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var data = await repository.QueryAsync(
                from trade in repository.DbContext.Trade
                join instrument in repository.DbContext.TradingInstrument
                    on trade.TradingInstrumentId equals instrument.Id
                join ruleCheck in repository.DbContext.TradeRuleCheck.Where(entity => entity.IsActive)
                    on trade.Id equals ruleCheck.TradeId into ruleChecks
                where trade.UserId == userId &&
                      instrument.UserId == userId &&
                      trade.IsActive &&
                      (trade.Status == TradeStatus.Closed || trade.Status == TradeStatus.BreakEven)
                select new HistoricalTrade
                {
                    StrategyId = trade.StrategyId,
                    Symbol = instrument.Symbol,
                    MarketType = instrument.MarketType,
                    Direction = trade.Direction,
                    Session = trade.Session,
                    TradeDate = trade.TradeDate,
                    StopLoss = trade.StopLoss,
                    TakeProfit = trade.TakeProfit,
                    RiskAmount = trade.RiskAmount,
                    PnL = trade.PnL,
                    RMultiple = trade.RMultiple,
                    RuleCheckCount = ruleChecks.Count(),
                    FollowedRuleCheckCount = ruleChecks.Count(ruleCheck => ruleCheck.IsFollowed)
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return (data.Records ?? [])
            .Select(CreateTrainingRow)
            .Where(row => row is not null)
            .Select(row => row!)
            .ToList();
    }

    private TradeOutcomeTrainingRow? CreateTrainingRow(HistoricalTrade trade)
    {
        var effectiveR = trade.RMultiple ??
            (trade.PnL.HasValue && trade.RiskAmount is > 0
                ? trade.PnL.Value / trade.RiskAmount.Value
                : null);

        if (!effectiveR.HasValue)
        {
            return null;
        }

        return featureBuilder.CreateFromTrade(
            trade.StrategyId,
            trade.Symbol,
            trade.MarketType,
            trade.Direction,
            trade.Session,
            trade.TradeDate,
            trade.StopLoss,
            trade.TakeProfit,
            trade.RiskAmount,
            trade.RuleCheckCount,
            trade.FollowedRuleCheckCount,
            effectiveR.Value > 0);
    }

    private sealed class HistoricalTrade
    {
        public Guid StrategyId { get; set; }
        public string Symbol { get; set; } = null!;
        public MarketType MarketType { get; set; }
        public TradeDirection Direction { get; set; }
        public TradingSession? Session { get; set; }
        public DateTime TradeDate { get; set; }
        public decimal? StopLoss { get; set; }
        public decimal? TakeProfit { get; set; }
        public decimal? RiskAmount { get; set; }
        public decimal? PnL { get; set; }
        public decimal? RMultiple { get; set; }
        public int RuleCheckCount { get; set; }
        public int FollowedRuleCheckCount { get; set; }
    }
}
