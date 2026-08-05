using MediatR;
using TradeX.Application.Abstractions.Configuration;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Abstractions.QueryParameters;
using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.Trades.Queries;

public sealed class GetTradesQuery
    : ContextualRequest,
      IRequest<StandardListResponse<GetTradesQueryResponseModel>>,
      IAuthenticatedRequest
{
    public PagingQueryParameters? PagingParameters { get; set; }
    public FilterQueryParameters? FilterParameters { get; set; }
    public SortQueryParameters? SortParameters { get; set; }
}

public sealed class GetTradesQueryHandler(
    ITradeXRepository repository,
    ApplicationConfiguration configuration)
    : IRequestHandler<GetTradesQuery, StandardListResponse<GetTradesQueryResponseModel>>
{
    public async Task<StandardListResponse<GetTradesQueryResponseModel>> Handle(
        GetTradesQuery request,
        CancellationToken cancellationToken)
    {
        var query = GetQuery(request);
        var sortParameters = GetSortParameters(request.SortParameters);

        var data = await repository.QueryAsync(
                query.OrderBySortParameters(sortParameters),
                pageIndex: request.PagingParameters?.Index ?? 0,
                pageSize: request.PagingParameters?.Size
                    ?? configuration.DataRetrievalConfiguration?.DefaultPageSize
                    ?? 10,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var records = data.Records ?? [];
        await PopulateTradingAccountsAsync(records, request.UserId(), cancellationToken)
            .ConfigureAwait(false);

        return new StandardListResponse<GetTradesQueryResponseModel>(
            records,
            data.TotalRecordCount,
            data.PageIndex,
            data.PageSize);
    }

    private IQueryable<GetTradesQueryResponseModel> GetQuery(GetTradesQuery request)
    {
        var userId = request.UserId();
        var filters = request.FilterParameters;

        var query =
            from trade in repository.DbContext.Trade
            join strategy in repository.DbContext.Strategy on trade.StrategyId equals strategy.Id
            join instrument in repository.DbContext.TradingInstrument on trade.TradingInstrumentId equals instrument.Id
            where trade.UserId == userId &&
                  strategy.UserId == userId &&
                  instrument.UserId == userId
            select new GetTradesQueryResponseModel
            {
                Id = trade.Id,
                StrategyId = trade.StrategyId,
                StrategyName = strategy.Name,
                TradingInstrumentId = trade.TradingInstrumentId,
                Symbol = instrument.Symbol,
                MarketType = instrument.MarketType,
                Direction = trade.Direction,
                Session = trade.Session,
                Status = trade.Status,
                TradeDate = trade.TradeDate,
                EntryPrice = trade.EntryPrice,
                ExitPrice = trade.ExitPrice,
                StopLoss = trade.StopLoss,
                TakeProfit = trade.TakeProfit,
                LotSize = trade.LotSize,
                RiskAmount = trade.RiskAmount,
                Pnl = trade.PnL,
                RMultiple = trade.RMultiple,
                Notes = trade.Notes,
                IsActive = trade.IsActive,
                CreatedAt = trade.CreatedAt
            };

        query = query
            .ApplyGuidFilter(filters?.GetGuidFilter("id"), x => x.Id)
            .ApplyGuidFilter(filters?.GetGuidFilter("strategyId"), x => x.StrategyId)
            .ApplyGuidFilter(filters?.GetGuidFilter("tradingInstrumentId"), x => x.TradingInstrumentId)
            .ApplyStringFilter(filters?.GetStringFilter("symbol"), x => x.Symbol)
            .ApplyStringFilter(filters?.GetStringFilter("notes"), x => x.Notes)
            .ApplyBoolFilter(filters?.GetBoolFilter("isActive"), x => x.IsActive)
            .ApplyDateFilter(filters?.GetDateFilter("tradeDate"), x => x.TradeDate);

        query = ApplyEnumFilters(query, filters);

        var tradingAccountId = filters?.GetGuidFilter("tradingAccountId")?.Eq;
        if (tradingAccountId.HasValue)
        {
            query = query.Where(trade => repository.DbContext.TradeAccountAssignment.Any(
                assignment => assignment.TradeId == trade.Id &&
                              assignment.TradingAccountId == tradingAccountId.Value));
        }

        return query;
    }

    private static IQueryable<GetTradesQueryResponseModel> ApplyEnumFilters(
        IQueryable<GetTradesQueryResponseModel> query,
        FilterQueryParameters? filters)
    {
        if (Enum.TryParse<MarketType>(filters?.GetStringFilter("marketType")?.Eq, true, out var marketType))
        {
            query = query.Where(x => x.MarketType == marketType);
        }

        if (Enum.TryParse<TradeStatus>(filters?.GetStringFilter("status")?.Eq, true, out var status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (Enum.TryParse<TradeDirection>(filters?.GetStringFilter("direction")?.Eq, true, out var direction))
        {
            query = query.Where(x => x.Direction == direction);
        }

        if (Enum.TryParse<TradingSession>(filters?.GetStringFilter("session")?.Eq, true, out var session))
        {
            query = query.Where(x => x.Session == session);
        }

        return query;
    }

    private static SortQueryParameters GetSortParameters(SortQueryParameters? sortParameters)
    {
        if (sortParameters?.Count > 0)
        {
            return sortParameters;
        }

        return
        [
            new SortQueryParameter(nameof(GetTradesQueryResponseModel.TradeDate), SortDirection.Desc),
            new SortQueryParameter(nameof(GetTradesQueryResponseModel.CreatedAt), SortDirection.Desc)
        ];
    }

    private async Task PopulateTradingAccountsAsync(
        List<GetTradesQueryResponseModel> trades,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var tradeIds = trades.Select(x => x.Id).ToList();
        if (tradeIds.Count == 0)
        {
            return;
        }

        var accountQuery =
            from assignment in repository.DbContext.TradeAccountAssignment
            join account in repository.DbContext.TradingAccount on assignment.TradingAccountId equals account.Id
            where tradeIds.Contains(assignment.TradeId) && account.UserId == userId
            select new TradeAccountRow
            {
                TradeId = assignment.TradeId,
                Id = account.Id,
                Name = account.Name
            };

        var data = await repository.QueryAsync(accountQuery, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var accountsByTradeId = (data.Records ?? [])
            .GroupBy(x => x.TradeId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(x => new TradeAccountResponseModel
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList());

        foreach (var trade in trades)
        {
            trade.TradingAccounts = accountsByTradeId.GetValueOrDefault(trade.Id, []);
        }
    }

    private sealed class TradeAccountRow
    {
        public Guid TradeId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}

public sealed class TradeAccountResponseModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public sealed class GetTradesQueryResponseModel
{
    public Guid Id { get; set; }
    public Guid StrategyId { get; set; }
    public string StrategyName { get; set; } = null!;
    public Guid TradingInstrumentId { get; set; }
    public string Symbol { get; set; } = null!;
    public MarketType MarketType { get; set; }
    public List<TradeAccountResponseModel> TradingAccounts { get; set; } = [];
    public TradeDirection Direction { get; set; }
    public TradingSession? Session { get; set; }
    public TradeStatus Status { get; set; }
    public DateTime TradeDate { get; set; }
    public decimal? EntryPrice { get; set; }
    public decimal? ExitPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public decimal? LotSize { get; set; }
    public decimal? RiskAmount { get; set; }
    public decimal? Pnl { get; set; }
    public decimal? RMultiple { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
