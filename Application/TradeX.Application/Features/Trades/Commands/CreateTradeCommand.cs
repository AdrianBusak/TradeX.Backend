using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;
using static TradeX.Application.Clients.Features.Trades.Commands.CreateTradeCommand;

namespace TradeX.Application.Clients.Features.Trades.Commands;

public sealed class CreateTradeCommand(CreateTradeCommandModel data)
    : BaseInput<CreateTradeCommandModel>(data),
      IRequest<StandardResponse<CreateEntityResponseModel>>,
      IAuthenticatedRequest
{
    public class CreateTradeCommandModel
    {
        public Guid StrategyId { get; set; }
        public Guid TradingInstrumentId { get; set; }
        public List<Guid> TradingAccountIds { get; set; } = [];
        public TradeDirection Direction { get; set; }
        public TradingSession? Session { get; set; }
        public TradeStatus Status { get; set; } = TradeStatus.Planned;
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
    }

    public sealed class CreateTradeCommandValidator : AbstractValidator<CreateTradeCommand>
    {
        public CreateTradeCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotEmpty()
                .SetValidator(new CreateTradeCommandModelValidator());
        }
    }

    public sealed class CreateTradeCommandModelValidator : AbstractValidator<CreateTradeCommandModel>
    {
        public CreateTradeCommandModelValidator()
        {
            RuleFor(x => x.StrategyId).NotEmpty();
            RuleFor(x => x.TradingInstrumentId).NotEmpty();
            RuleFor(x => x.TradingAccountIds).NotEmpty().Must(x => x.Distinct().Count() == x.Count);
            RuleForEach(x => x.TradingAccountIds).NotEmpty();
            RuleFor(x => x.Direction).IsInEnum();
            RuleFor(x => x.Status).IsInEnum();
            RuleFor(x => x.TradeDate).NotEmpty();
            RuleFor(x => x.Notes).MaximumLength(4000);
            RuleFor(x => x.EntryPrice).GreaterThan(0).When(x => x.EntryPrice.HasValue);
            RuleFor(x => x.ExitPrice).GreaterThan(0).When(x => x.ExitPrice.HasValue);
            RuleFor(x => x.StopLoss).GreaterThan(0).When(x => x.StopLoss.HasValue);
            RuleFor(x => x.TakeProfit).GreaterThan(0).When(x => x.TakeProfit.HasValue);
            RuleFor(x => x.LotSize).GreaterThan(0).When(x => x.LotSize.HasValue);
            RuleFor(x => x.RiskAmount).GreaterThanOrEqualTo(0).When(x => x.RiskAmount.HasValue);
        }
    }
}

public sealed class CreateTradeCommandHandler(ITradeXRepository repository)
    : IRequestHandler<CreateTradeCommand, StandardResponse<CreateEntityResponseModel>>
{
    public async Task<StandardResponse<CreateEntityResponseModel>> Handle(CreateTradeCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        var strategyExists = (await repository
            .GetIdAsync<Strategy>(x => x.Id == request.Model.StrategyId
                                && x.UserId == userId, cancellationToken).ConfigureAwait(false)).HasValue;
        
        var instrumentExists = (await repository
            .GetIdAsync<TradingInstrument>(x => x.Id == request.Model.TradingInstrumentId
                                && x.UserId == userId && x.IsActive, cancellationToken).ConfigureAwait(false)).HasValue;

        if (!strategyExists || !instrumentExists)
        {
            return new StandardResponse<CreateEntityResponseModel>(OperationResult.NotFound, "Related entity was not found.", null!);
        }

        var accountIds = request.Model.TradingAccountIds.Distinct().ToList();
        var accountQuery = from account in repository.DbContext.TradingAccount
                           where account.UserId == userId
                           && accountIds.Contains(account.Id)
                           select account;
        var accounts = await repository.QueryAsync(accountQuery, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (accounts.Records?.Count != accountIds.Count)
        {
            return new StandardResponse<CreateEntityResponseModel>(OperationResult.NotFound, "One or more trading accounts were not found.", null!);
        }

        var trade = new Trade
        {
            UserId = userId,
            StrategyId = request.Model.StrategyId,
            TradingInstrumentId = request.Model.TradingInstrumentId,
            Direction = request.Model.Direction,
            Session = request.Model.Session,
            Status = request.Model.Status,
            TradeDate = request.Model.TradeDate,
            EntryPrice = request.Model.EntryPrice,
            ExitPrice = request.Model.ExitPrice,
            StopLoss = request.Model.StopLoss,
            TakeProfit = request.Model.TakeProfit,
            LotSize = request.Model.LotSize,
            RiskAmount = request.Model.RiskAmount,
            PnL = request.Model.Pnl,
            RMultiple = request.Model.RMultiple,
            Notes = string.IsNullOrWhiteSpace(request.Model.Notes) ? null : request.Model.Notes.Trim(),
            IsActive = true,
            CreatedByUserId = userId,
            ModifiedByUserId = userId
        };

        var tradeId = await repository.AddAsync(trade, cancellationToken).ConfigureAwait(false);
        var assignments = accountIds.Select(accountId => new TradeAccountAssignment
        {
            TradeId = tradeId,
            TradingAccountId = accountId,
            IsActive = true,
            CreatedByUserId = userId,
            ModifiedByUserId = userId
        }).ToList();
        await repository.AddRangeAsync(assignments, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<CreateEntityResponseModel>(OperationResult.Created, new CreateEntityResponseModel { Id = tradeId });
    }
}
