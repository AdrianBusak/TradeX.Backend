using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.Trades.Queries;

public sealed class GetTradeByIdQuery(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<GetTradeByIdResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class Validator : AbstractValidator<GetTradeByIdQuery>
    {
        public Validator() => RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class GetTradeByIdQueryHandler(ITradeXRepository repository)
    : IRequestHandler<GetTradeByIdQuery, StandardResponse<GetTradeByIdResponseModel>>
{
    public async Task<StandardResponse<GetTradeByIdResponseModel>> Handle(
        GetTradeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var result = await repository.QueryAsync(
            from trade in repository.DbContext.Trade
            join strategy in repository.DbContext.Strategy on trade.StrategyId equals strategy.Id
            join instrument in repository.DbContext.TradingInstrument on trade.TradingInstrumentId equals instrument.Id
            where trade.Id == request.Id && trade.UserId == userId && strategy.UserId == userId && instrument.UserId == userId
            select new GetTradeByIdResponseModel
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
                IsActive = trade.IsActive
            },
            pageSize: 1,
            cancellationToken: cancellationToken);

        var model = result.Records?.FirstOrDefault();
        if (model is null)
        {
            return new StandardResponse<GetTradeByIdResponseModel>(
                OperationResult.NotFound,
                "Trade was not found.",
                null!);
        }

        var accounts = await repository.QueryAsync(
            from assignment in repository.DbContext.TradeAccountAssignment
            join account in repository.DbContext.TradingAccount on assignment.TradingAccountId equals account.Id
            where assignment.TradeId == model.Id && account.UserId == userId
            select new TradeAccountResponseModel { Id = account.Id, Name = account.Name },
            cancellationToken: cancellationToken);

        model.TradingAccounts = accounts.Records ?? [];
        model.TradingAccountIds = model.TradingAccounts.Select(x => x.Id).ToList();

        return new StandardResponse<GetTradeByIdResponseModel>(OperationResult.Ok, model);
    }
}

public sealed class GetTradeByIdResponseModel
{
    public Guid Id { get; set; }
    public Guid StrategyId { get; set; }
    public string StrategyName { get; set; } = null!;
    public Guid TradingInstrumentId { get; set; }
    public string Symbol { get; set; } = null!;
    public MarketType MarketType { get; set; }
    public List<Guid> TradingAccountIds { get; set; } = [];
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
}
