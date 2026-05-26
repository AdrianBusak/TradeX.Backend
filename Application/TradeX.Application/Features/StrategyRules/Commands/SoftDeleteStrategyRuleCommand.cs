using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.StrategyRules.Commands;

public sealed class SoftDeleteStrategyRuleCommand(Guid strategyId, Guid ruleId)
    : ContextualRequest,
      IRequest<StandardResponse<SoftDeleteEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid StrategyId { get; } = strategyId;
    public Guid RuleId { get; } = ruleId;

    public sealed class SoftDeleteStrategyRuleCommandValidator : AbstractValidator<SoftDeleteStrategyRuleCommand>
    {
        public SoftDeleteStrategyRuleCommandValidator()
        {
            RuleFor(x => x.StrategyId)
                .NotEmpty();

            RuleFor(x => x.RuleId)
                .NotEmpty();
        }
    }
}

public sealed class SoftDeleteStrategyRuleCommandHandler(ITradeXRepository repository)
    : IRequestHandler<SoftDeleteStrategyRuleCommand, StandardResponse<SoftDeleteEntityResponseModel>>
{
    public async Task<StandardResponse<SoftDeleteEntityResponseModel>> Handle(
        SoftDeleteStrategyRuleCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        if (!await StrategyExistsAsync(request.StrategyId, userId, cancellationToken).ConfigureAwait(false))
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<SoftDeleteEntityResponseModel>(
                request.StrategyId,
                nameof(Strategy));
        }

        var entity = await repository.GetSingleAsync<StrategyRule>(
                rule => rule.Id == request.RuleId && rule.StrategyId == request.StrategyId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<SoftDeleteEntityResponseModel>(
                request.RuleId,
                nameof(StrategyRule));
        }

        if (!entity.IsActive)
        {
            return new StandardResponse<SoftDeleteEntityResponseModel>(
                OperationResult.BadRequest,
                "Entity is already deleted.",
                null!);
        }

        entity.IsActive = false;
        entity.ModifiedByUserId = userId;

        await repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<SoftDeleteEntityResponseModel>(
            OperationResult.Deleted,
            new SoftDeleteEntityResponseModel());
    }

    private async Task<bool> StrategyExistsAsync(
        Guid strategyId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var id = await repository.GetIdAsync<Strategy>(
                strategy => strategy.Id == strategyId && strategy.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        return id.HasValue;
    }
}
