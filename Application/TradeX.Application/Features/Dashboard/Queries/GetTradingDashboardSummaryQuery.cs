using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.Dashboard.Queries;

public enum TradingDashboardPeriod
{
    ThisMonth = 1,
    LastMonth = 2,
    AllTime = 3
}

public sealed class GetTradingDashboardSummaryQuery
    : ContextualRequest,
      IRequest<StandardResponse<TradingDashboardSummaryResponse>>,
      IAuthenticatedRequest
{
    public TradingDashboardPeriod? Period { get; set; }

    public DateOnly? DateFrom { get; set; }

    public DateOnly? DateTo { get; set; }

    public Guid? StrategyId { get; set; }

    public Guid? AccountId { get; set; }

    public sealed class GetTradingDashboardSummaryQueryValidator
        : AbstractValidator<GetTradingDashboardSummaryQuery>
    {
        public GetTradingDashboardSummaryQueryValidator()
        {
            RuleFor(x => x)
                .Must(x => !x.DateFrom.HasValue ||
                           !x.DateTo.HasValue ||
                           x.DateFrom <= x.DateTo)
                .WithMessage("DateFrom must be before or equal to DateTo.");
        }
    }
}

public sealed class GetTradingDashboardSummaryQueryHandler(ITradeXRepository repository)
    : IRequestHandler<
        GetTradingDashboardSummaryQuery,
        StandardResponse<TradingDashboardSummaryResponse>>
{
    public async Task<StandardResponse<TradingDashboardSummaryResponse>> Handle(
        GetTradingDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var query = GetQuery(request);

        var data = await repository.QueryAsync(
                query,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var response = CreateResponse(data.Records ?? []);

        return new StandardResponse<TradingDashboardSummaryResponse>(
            OperationResult.Ok,
            response);
    }

    private IQueryable<TradingDashboardTradeRow> GetQuery(
        GetTradingDashboardSummaryQuery request)
    {
        var userId = request.UserId();
        var dateRange = GetDateRange(request);

        var query =
            from trade in repository.DbContext.Trade
            join strategy in repository.DbContext.Strategy on trade.StrategyId equals strategy.Id
            join instrument in repository.DbContext.TradingInstrument on trade.TradingInstrumentId equals instrument.Id
            where trade.UserId == userId &&
                  strategy.UserId == userId &&
                  instrument.UserId == userId &&
                  trade.IsActive
            select new TradingDashboardTradeRow
            {
                Id = trade.Id,
                StrategyId = strategy.Id,
                StrategyName = strategy.Name,
                TradingInstrumentId = instrument.Id,
                Symbol = instrument.Symbol,
                Direction = trade.Direction,
                Session = trade.Session,
                Status = trade.Status,
                TradeDate = trade.TradeDate,
                Pnl = trade.PnL,
                RMultiple = trade.RMultiple,
                RiskAmount = trade.RiskAmount
            };

        if (dateRange.DateFrom.HasValue)
        {
            query = query.Where(x => x.TradeDate >= dateRange.DateFrom.Value);
        }

        if (dateRange.DateToExclusive.HasValue)
        {
            query = query.Where(x => x.TradeDate < dateRange.DateToExclusive.Value);
        }

        if (request.StrategyId.HasValue)
        {
            query = query.Where(x => x.StrategyId == request.StrategyId.Value);
        }

        if (request.AccountId.HasValue)
        {
            query = query.Where(trade =>
                (from assignment in repository.DbContext.TradeAccountAssignment
                 join account in repository.DbContext.TradingAccount
                     on assignment.TradingAccountId equals account.Id
                 where assignment.TradeId == trade.Id &&
                       account.Id == request.AccountId.Value &&
                       account.UserId == userId
                 select assignment.Id)
                .Any());
        }

        return query;
    }

    private static DashboardDateRange GetDateRange(
        GetTradingDashboardSummaryQuery request)
    {
        if (request.DateFrom.HasValue || request.DateTo.HasValue)
        {
            return new DashboardDateRange(
                request.DateFrom?.ToDateTime(TimeOnly.MinValue),
                request.DateTo?.AddDays(1).ToDateTime(TimeOnly.MinValue));
        }

        var period = request.Period ?? TradingDashboardPeriod.ThisMonth;

        if (period == TradingDashboardPeriod.AllTime)
        {
            return new DashboardDateRange(null, null);
        }

        var currentDate = DateTime.UtcNow;
        var currentMonthStart = new DateTime(currentDate.Year, currentDate.Month, 1);

        if (period == TradingDashboardPeriod.LastMonth)
        {
            var previousMonthStart = currentMonthStart.AddMonths(-1);

            return new DashboardDateRange(previousMonthStart, currentMonthStart);
        }

        return new DashboardDateRange(
            currentMonthStart,
            currentMonthStart.AddMonths(1));
    }

    private static TradingDashboardSummaryResponse CreateResponse(
        List<TradingDashboardTradeRow> trades)
    {
        var topStrategies = trades
            .GroupBy(x => new { x.StrategyId, x.StrategyName })
            .Select(group => CreateStrategySummary(group.Key.StrategyId, group.Key.StrategyName, group))
            .OrderByDescending(x => x.TotalR)
            .ThenByDescending(x => x.TotalPnl)
            .Take(3)
            .ToList();

        var sessionSummaries = trades
            .Where(x => x.Session.HasValue)
            .GroupBy(x => x.Session!.Value)
            .Select(group => CreateSessionSummary(group.Key, group))
            .OrderByDescending(x => x.TotalR)
            .ThenByDescending(x => x.TotalPnl)
            .ToList();

        var response = new TradingDashboardSummaryResponse
        {
            TotalTrades = trades.Count,
            TotalPnl = trades.Sum(x => x.Pnl ?? 0),
            TotalR = trades.Sum(x => x.EffectiveR ?? 0),
            WinRate = CalculateWinRate(trades),
            AverageR = GetAverage(trades.Where(x => x.EffectiveR.HasValue).Select(x => x.EffectiveR!.Value)),
            AverageWin = GetAverage(trades.Where(x => x.Pnl > 0).Select(x => x.Pnl!.Value)),
            AverageLoss = GetAverage(trades.Where(x => x.Pnl < 0).Select(x => x.Pnl!.Value)),
            BestStrategy = topStrategies.FirstOrDefault(),
            BestSession = sessionSummaries.FirstOrDefault(),
            RCurve = CreateRCurve(trades),
            RecentTrades = trades
                .OrderByDescending(x => x.TradeDate)
                .Take(5)
                .Select(x => new DashboardRecentTrade
                {
                    Id = x.Id,
                    Symbol = x.Symbol,
                    StrategyName = x.StrategyName,
                    Direction = x.Direction,
                    Session = x.Session,
                    Status = x.Status,
                    TradeDate = x.TradeDate,
                    Pnl = x.Pnl,
                    RMultiple = x.EffectiveR
                })
                .ToList(),
            TopStrategies = topStrategies
        };

        response.Insights = CreateInsights(response, sessionSummaries, trades);

        return response;
    }

    private static DashboardStrategySummary CreateStrategySummary(
        Guid strategyId,
        string strategyName,
        IEnumerable<TradingDashboardTradeRow> trades)
    {
        var records = trades.ToList();

        return new DashboardStrategySummary
        {
            StrategyId = strategyId,
            StrategyName = strategyName,
            TradeCount = records.Count,
            TotalPnl = records.Sum(x => x.Pnl ?? 0),
            TotalR = records.Sum(x => x.EffectiveR ?? 0),
            WinRate = CalculateWinRate(records),
            AverageR = GetAverage(records.Where(x => x.EffectiveR.HasValue).Select(x => x.EffectiveR!.Value))
        };
    }

    private static DashboardSessionSummary CreateSessionSummary(
        TradingSession session,
        IEnumerable<TradingDashboardTradeRow> trades)
    {
        var records = trades.ToList();

        return new DashboardSessionSummary
        {
            Session = session,
            TradeCount = records.Count,
            TotalPnl = records.Sum(x => x.Pnl ?? 0),
            TotalR = records.Sum(x => x.EffectiveR ?? 0),
            WinRate = CalculateWinRate(records),
            AverageR = GetAverage(records.Where(x => x.EffectiveR.HasValue).Select(x => x.EffectiveR!.Value))
        };
    }

    private static List<DashboardCurvePoint> CreateRCurve(
        List<TradingDashboardTradeRow> trades)
    {
        var cumulativeR = 0m;

        return trades
            .Where(x => x.EffectiveR.HasValue)
            .GroupBy(x => DateOnly.FromDateTime(x.TradeDate))
            .OrderBy(x => x.Key)
            .Select(group => new DashboardCurvePoint
            {
                Date = group.Key,
                Value = cumulativeR += group.Sum(x => x.EffectiveR!.Value)
            })
            .ToList();
    }

    private static decimal? CalculateWinRate(IEnumerable<TradingDashboardTradeRow> trades)
    {
        var results = trades
            .Select(GetTradeResult)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();

        if (results.Count == 0)
        {
            return null;
        }

        return results.Count(x => x > 0) * 100m / results.Count;
    }

    private static decimal? GetTradeResult(TradingDashboardTradeRow trade)
    {
        return trade.Pnl ?? trade.RMultiple;
    }

    private static decimal GetAverage(IEnumerable<decimal> values)
    {
        var records = values.ToList();

        return records.Count == 0
            ? 0
            : records.Average();
    }

    private static List<string> CreateInsights(
        TradingDashboardSummaryResponse response,
        List<DashboardSessionSummary> sessionSummaries,
        List<TradingDashboardTradeRow> trades)
    {
        var insights = new List<string>();

        if (response.BestStrategy is not null)
        {
            insights.Add($"Best strategy: {response.BestStrategy.StrategyName}");
        }

        if (response.BestSession is not null)
        {
            insights.Add($"Best performing session: {response.BestSession.Session}");
        }

        var weakestSession = sessionSummaries
            .OrderBy(x => x.TotalR)
            .ThenBy(x => x.TotalPnl)
            .FirstOrDefault();

        if (weakestSession is not null &&
            (weakestSession.TotalR < 0 || weakestSession.TotalPnl < 0))
        {
            insights.Add($"Weakest session: {weakestSession.Session}");
        }

        var mostTradedInstrument = trades
            .GroupBy(x => x.Symbol)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => group.Key)
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(mostTradedInstrument))
        {
            insights.Add($"Most traded instrument: {mostTradedInstrument}");
        }

        return insights;
    }

    private sealed record DashboardDateRange(
        DateTime? DateFrom,
        DateTime? DateToExclusive);

    private sealed class TradingDashboardTradeRow
    {
        public Guid Id { get; set; }

        public Guid StrategyId { get; set; }

        public string StrategyName { get; set; } = null!;

        public Guid TradingInstrumentId { get; set; }

        public string Symbol { get; set; } = null!;

        public TradeDirection Direction { get; set; }

        public TradingSession? Session { get; set; }

        public TradeStatus Status { get; set; }

        public DateTime TradeDate { get; set; }

        public decimal? Pnl { get; set; }

        public decimal? RMultiple { get; set; }

        public decimal? RiskAmount { get; set; }

        public decimal? EffectiveR => RMultiple ??
            (Pnl.HasValue && RiskAmount is > 0 ? Pnl.Value / RiskAmount.Value : null);
    }
}

public sealed class TradingDashboardSummaryResponse
{
    public int TotalTrades { get; set; }

    public decimal TotalPnl { get; set; }

    public decimal TotalR { get; set; }

    public decimal? WinRate { get; set; }

    public decimal AverageR { get; set; }

    public decimal? AverageWin { get; set; }

    public decimal? AverageLoss { get; set; }

    public DashboardStrategySummary? BestStrategy { get; set; }

    public DashboardSessionSummary? BestSession { get; set; }

    public List<DashboardCurvePoint> RCurve { get; set; } = [];

    public List<DashboardRecentTrade> RecentTrades { get; set; } = [];

    public List<DashboardStrategySummary> TopStrategies { get; set; } = [];

    public List<string> Insights { get; set; } = [];
}

public sealed class DashboardCurvePoint
{
    public DateOnly Date { get; set; }

    public decimal Value { get; set; }
}

public sealed class DashboardRecentTrade
{
    public Guid Id { get; set; }

    public string Symbol { get; set; } = null!;

    public string StrategyName { get; set; } = null!;

    public TradeDirection Direction { get; set; }

    public TradingSession? Session { get; set; }

    public TradeStatus Status { get; set; }

    public DateTime TradeDate { get; set; }

    public decimal? Pnl { get; set; }

    public decimal? RMultiple { get; set; }
}

public sealed class DashboardStrategySummary
{
    public Guid StrategyId { get; set; }

    public string StrategyName { get; set; } = null!;

    public int TradeCount { get; set; }

    public decimal TotalPnl { get; set; }

    public decimal TotalR { get; set; }

    public decimal? WinRate { get; set; }

    public decimal AverageR { get; set; }
}

public sealed class DashboardSessionSummary
{
    public TradingSession? Session { get; set; }

    public int TradeCount { get; set; }

    public decimal TotalPnl { get; set; }

    public decimal TotalR { get; set; }

    public decimal? WinRate { get; set; }

    public decimal AverageR { get; set; }
}
