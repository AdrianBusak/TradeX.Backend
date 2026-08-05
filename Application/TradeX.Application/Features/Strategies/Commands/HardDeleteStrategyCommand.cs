using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Strategies.Commands;

public sealed class HardDeleteStrategyCommand(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<HardDeleteEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class HardDeleteStrategyCommandValidator : AbstractValidator<HardDeleteStrategyCommand>
    {
        public HardDeleteStrategyCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}

public sealed class HardDeleteStrategyCommandHandler(ITradeXRepository repository)
    : IRequestHandler<HardDeleteStrategyCommand, StandardResponse<HardDeleteEntityResponseModel>>
{
    public async Task<StandardResponse<HardDeleteEntityResponseModel>> Handle(
        HardDeleteStrategyCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        var entity = await repository.GetSingleAsync<Strategy>(
                strategy => strategy.Id == request.Id && strategy.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<HardDeleteEntityResponseModel>(
                request.Id,
                nameof(Strategy));
        }

        if (await HasRulesAsync(request.Id, cancellationToken).ConfigureAwait(false))
        {
            return new StandardResponse<HardDeleteEntityResponseModel>(
                OperationResult.BadRequest,
                "Entity has related records.",
                null!);
        }

        if (await HasTradesAsync(request.Id, cancellationToken).ConfigureAwait(false))
        {
            return new StandardResponse<HardDeleteEntityResponseModel>(
                OperationResult.BadRequest,
                "Entity has related records.",
                null!);
        }

        await repository.DeleteHardAsync<Strategy>(request.Id, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<HardDeleteEntityResponseModel>(
            OperationResult.Deleted,
            new HardDeleteEntityResponseModel());
    }

    private async Task<bool> HasRulesAsync(Guid strategyId, CancellationToken cancellationToken)
    {
        var query =
            from rule in repository.DbContext.StrategyRule
            where rule.StrategyId == strategyId
            select new RelatedEntityResponseModel
            {
                Id = rule.Id
            };

        var result = await repository.QueryAsync(
                query,
                pageSize: 1,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result.Records is { Count: > 0 };
    }

    private async Task<bool> HasTradesAsync(Guid strategyId, CancellationToken cancellationToken)
    {
        var query =
            from trade in repository.DbContext.Trade
            where trade.StrategyId == strategyId
            select new RelatedEntityResponseModel
            {
                Id = trade.Id
            };

        var result = await repository.QueryAsync(
                query,
                pageSize: 1,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result.Records is { Count: > 0 };
    }

    private sealed class RelatedEntityResponseModel
    {
        public Guid Id { get; set; }
    }
}
