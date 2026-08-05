using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using static TradeX.Application.Clients.Features.Trades.Commands.CreateTradeCommand;
using static TradeX.Application.Clients.Features.Trades.Commands.UpdateTradeCommand;

namespace TradeX.Application.Clients.Features.Trades.Commands;

public sealed class UpdateTradeCommand(Guid id, UpdateTradeCommandModel data)
    : BaseInput<UpdateTradeCommandModel>(data),
      IRequest<StandardResponse<UpdateEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class UpdateTradeCommandModel : CreateTradeCommandModel
    {
    }

    public sealed class UpdateTradeCommandValidator : AbstractValidator<UpdateTradeCommand>
    {
        public UpdateTradeCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Model)
                .NotEmpty()
                .SetValidator(new CreateTradeCommandModelValidator());
        }
    }
}

public sealed class UpdateTradeCommandHandler(ITradeXRepository repository)
    : IRequestHandler<UpdateTradeCommand, StandardResponse<UpdateEntityResponseModel>>
{
    public async Task<StandardResponse<UpdateEntityResponseModel>> Handle(UpdateTradeCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var trade = await repository.GetSingleAsync<Trade>(
                entity => entity.Id == request.Id && entity.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (trade is null)
        {
            return new StandardResponse<UpdateEntityResponseModel>(
                OperationResult.NotFound,
                "Trade was not found.",
                null!);
        }

        var strategyExists = (await repository.GetIdAsync<Strategy>(
                entity => entity.Id == request.Model.StrategyId && entity.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false)).HasValue;

        var instrumentExists = (await repository.GetIdAsync<TradingInstrument>(
                entity => entity.Id == request.Model.TradingInstrumentId && entity.UserId == userId && entity.IsActive,
                cancellationToken)
            .ConfigureAwait(false)).HasValue;

        if (!strategyExists || !instrumentExists)
        {
            return new StandardResponse<UpdateEntityResponseModel>(
                OperationResult.NotFound,
                "Related entity was not found.",
                null!);
        }

        var accountIds = request.Model.TradingAccountIds.Distinct().ToList();
        var accountQuery =
            from account in repository.DbContext.TradingAccount
            where account.UserId == userId && accountIds.Contains(account.Id)
            select account;

        var accounts = await repository.QueryAsync(accountQuery, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (accounts.Records?.Count != accountIds.Count)
        {
            return new StandardResponse<UpdateEntityResponseModel>(
                OperationResult.NotFound,
                "One or more trading accounts were not found.",
                null!);
        }

        trade.StrategyId = request.Model.StrategyId;
        trade.TradingInstrumentId = request.Model.TradingInstrumentId;
        trade.Direction = request.Model.Direction;
        trade.Session = request.Model.Session;
        trade.Status = request.Model.Status;
        trade.TradeDate = request.Model.TradeDate;
        trade.EntryPrice = request.Model.EntryPrice;
        trade.ExitPrice = request.Model.ExitPrice;
        trade.StopLoss = request.Model.StopLoss;
        trade.TakeProfit = request.Model.TakeProfit;
        trade.LotSize = request.Model.LotSize;
        trade.RiskAmount = request.Model.RiskAmount;
        trade.PnL = request.Model.Pnl;
        trade.RMultiple = request.Model.RMultiple;
        trade.Notes = string.IsNullOrWhiteSpace(request.Model.Notes) ? null : request.Model.Notes.Trim();
        trade.ModifiedByUserId = userId;

        var assignments = await repository.GetListAsync<TradeAccountAssignment>(
                cancellationToken,
                entity => entity.TradeId == trade.Id)
            .ConfigureAwait(false);

        if (assignments.Count > 0)
        {
            await repository.DeleteHardRangeAsync<TradeAccountAssignment>(
                    assignments.Select(x => x.Id).ToList(),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        var newAssignments = accountIds
            .Select(accountId => new TradeAccountAssignment
            {
                TradeId = trade.Id,
                TradingAccountId = accountId,
                IsActive = true,
                CreatedByUserId = userId,
                ModifiedByUserId = userId
            })
            .ToList();

        await repository.AddRangeAsync(newAssignments, cancellationToken)
            .ConfigureAwait(false);
        await repository.UpdateAsync(trade, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<UpdateEntityResponseModel>(OperationResult.Updated, new UpdateEntityResponseModel());
    }
}
