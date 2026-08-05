using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.StrategyRules.Commands;

public sealed class RestoreStrategyRuleCommand(Guid strategyId, Guid ruleId)
    : ContextualRequest,
      IRequest<StandardResponse<RestoreEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid StrategyId { get; } = strategyId;
    public Guid RuleId { get; } = ruleId;

    public sealed class RestoreStrategyRuleCommandValidator
        : AbstractValidator<RestoreStrategyRuleCommand>
    {
        public RestoreStrategyRuleCommandValidator()
        {
            RuleFor(x => x.StrategyId).NotEmpty();
            RuleFor(x => x.RuleId).NotEmpty();
        }
    }
}

public sealed class RestoreStrategyRuleCommandHandler(ITradeXRepository repository)
    : IRequestHandler<RestoreStrategyRuleCommand, StandardResponse<RestoreEntityResponseModel>>
{
    public async Task<StandardResponse<RestoreEntityResponseModel>> Handle(
        RestoreStrategyRuleCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var strategy = await repository.GetSingleAsync<Strategy>(
                entity => entity.Id == request.StrategyId && entity.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (strategy is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<RestoreEntityResponseModel>(
                request.StrategyId,
                nameof(Strategy));
        }

        var rule = await repository.GetSingleAsync<StrategyRule>(
                entity => entity.Id == request.RuleId && entity.StrategyId == strategy.Id,
                cancellationToken)
            .ConfigureAwait(false);

        if (rule is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<RestoreEntityResponseModel>(
                request.RuleId,
                nameof(StrategyRule));
        }

        if (rule.IsActive)
        {
            return new StandardResponse<RestoreEntityResponseModel>(
                OperationResult.BadRequest,
                "Entity is already active.",
                null!);
        }

        rule.IsActive = true;
        rule.ModifiedByUserId = userId;

        await repository.UpdateAsync(rule, cancellationToken)
            .ConfigureAwait(false);

        return new StandardResponse<RestoreEntityResponseModel>(
            OperationResult.Updated,
            new RestoreEntityResponseModel());
    }
}
