using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;
using static TradeX.Application.Clients.Features.StrategyRules.Commands.UpdateStrategyRuleCommand;

namespace TradeX.Application.Clients.Features.StrategyRules.Commands;

public sealed class UpdateStrategyRuleCommand(Guid strategyId, Guid ruleId, UpdateStrategyRuleCommandModel data)
    : BaseInput<UpdateStrategyRuleCommandModel>(data),
      IRequest<StandardResponse<UpdateEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid StrategyId { get; } = strategyId;
    public Guid RuleId { get; } = ruleId;

    public sealed class UpdateStrategyRuleCommandModel
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsRequired { get; set; }
        public StrategyRuleCategory Category { get; set; } = StrategyRuleCategory.Entry;
        public StrategyRuleImportance Importance { get; set; } = StrategyRuleImportance.Medium;
    }

    public sealed class UpdateStrategyRuleCommandValidator : AbstractValidator<UpdateStrategyRuleCommand>
    {
        public UpdateStrategyRuleCommandValidator()
        {
            RuleFor(x => x.StrategyId)
                .NotEmpty();

            RuleFor(x => x.RuleId)
                .NotEmpty();

            RuleFor(x => x.Model)
                .NotEmpty()
                .SetValidator(new UpdateStrategyRuleCommandModelValidator());
        }
    }

    public sealed class UpdateStrategyRuleCommandModelValidator : AbstractValidator<UpdateStrategyRuleCommandModel>
    {
        public UpdateStrategyRuleCommandModelValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(2000);

            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Category)
                .IsInEnum();

            RuleFor(x => x.Importance)
                .IsInEnum();
        }
    }
}

public sealed class UpdateStrategyRuleCommandHandler(ITradeXRepository repository)
    : IRequestHandler<UpdateStrategyRuleCommand, StandardResponse<UpdateEntityResponseModel>>
{
    public async Task<StandardResponse<UpdateEntityResponseModel>> Handle(
        UpdateStrategyRuleCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        if (!await StrategyExistsAsync(request.StrategyId, userId, cancellationToken).ConfigureAwait(false))
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<UpdateEntityResponseModel>(
                request.StrategyId,
                nameof(Strategy));
        }

        var entity = await repository.GetSingleAsync<StrategyRule>(
                rule => rule.Id == request.RuleId && rule.StrategyId == request.StrategyId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<UpdateEntityResponseModel>(
                request.RuleId,
                nameof(StrategyRule));
        }

        entity.Title = NormalizeRequired(request.Model.Title);
        entity.Description = NormalizeOptional(request.Model.Description);
        entity.Order = request.Model.Order;
        entity.IsRequired = request.Model.IsRequired;
        entity.Category = request.Model.Category;
        entity.Importance = request.Model.Importance;
        entity.ModifiedByUserId = userId;

        await repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<UpdateEntityResponseModel>(
            OperationResult.Updated,
            new UpdateEntityResponseModel());
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

    private static string NormalizeRequired(string value)
        => value.Trim();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
