using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.StrategyRules.Commands;

public sealed class HardDeleteStrategyRuleCommand(Guid strategyId, Guid ruleId)
    : ContextualRequest,
      IRequest<StandardResponse<HardDeleteEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid StrategyId { get; } = strategyId;
    public Guid RuleId { get; } = ruleId;

    public sealed class HardDeleteStrategyRuleCommandValidator
        : AbstractValidator<HardDeleteStrategyRuleCommand>
    {
        public HardDeleteStrategyRuleCommandValidator()
        {
            RuleFor(x => x.StrategyId)
                .NotEmpty();

            RuleFor(x => x.RuleId)
                .NotEmpty();
        }
    }
}

public sealed class HardDeleteStrategyRuleCommandHandler(ITradeXRepository repository)
    : IRequestHandler<HardDeleteStrategyRuleCommand, StandardResponse<HardDeleteEntityResponseModel>>
{
    public async Task<StandardResponse<HardDeleteEntityResponseModel>> Handle(
        HardDeleteStrategyRuleCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        if (!await StrategyExistsAsync(request.StrategyId, userId, cancellationToken).ConfigureAwait(false))
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<HardDeleteEntityResponseModel>(
                request.StrategyId,
                nameof(Strategy));
        }

        var entity = await repository.GetSingleAsync<StrategyRule>(
                rule => rule.Id == request.RuleId && rule.StrategyId == request.StrategyId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<HardDeleteEntityResponseModel>(
                request.RuleId,
                nameof(StrategyRule));
        }

        await repository.DeleteHardAsync<StrategyRule>(request.RuleId, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<HardDeleteEntityResponseModel>(
            OperationResult.Deleted,
            new HardDeleteEntityResponseModel());
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
