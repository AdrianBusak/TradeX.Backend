using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;
using static TradeX.Application.Clients.Features.StrategyRules.Commands.CreateStrategyRuleCommand;

namespace TradeX.Application.Clients.Features.StrategyRules.Commands;

public sealed class CreateStrategyRuleCommand(Guid strategyId, CreateStrategyRuleCommandModel data)
    : BaseInput<CreateStrategyRuleCommandModel>(data),
      IRequest<StandardResponse<CreateEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid StrategyId { get; } = strategyId;

    public sealed class CreateStrategyRuleCommandModel
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsRequired { get; set; }
        public StrategyRuleCategory Category { get; set; } = StrategyRuleCategory.Entry;
        public StrategyRuleImportance Importance { get; set; } = StrategyRuleImportance.Medium;
    }

    public sealed class CreateStrategyRuleCommandValidator : AbstractValidator<CreateStrategyRuleCommand>
    {
        public CreateStrategyRuleCommandValidator()
        {
            RuleFor(x => x.StrategyId)
                .NotEmpty();

            RuleFor(x => x.Model)
                .NotEmpty()
                .SetValidator(new CreateStrategyRuleCommandModelValidator());
        }
    }

    public sealed class CreateStrategyRuleCommandModelValidator : AbstractValidator<CreateStrategyRuleCommandModel>
    {
        public CreateStrategyRuleCommandModelValidator()
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

public sealed class CreateStrategyRuleCommandHandler(ITradeXRepository repository)
    : IRequestHandler<CreateStrategyRuleCommand, StandardResponse<CreateEntityResponseModel>>
{
    public async Task<StandardResponse<CreateEntityResponseModel>> Handle(
        CreateStrategyRuleCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        if (!await StrategyExistsAsync(request.StrategyId, userId, cancellationToken).ConfigureAwait(false))
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<CreateEntityResponseModel>(
                request.StrategyId,
                nameof(Strategy));
        }

        var entity = new StrategyRule
        {
            StrategyId = request.StrategyId,
            Title = NormalizeRequired(request.Model.Title),
            Description = NormalizeOptional(request.Model.Description),
            Order = request.Model.Order,
            IsRequired = request.Model.IsRequired,
            Category = request.Model.Category,
            Importance = request.Model.Importance,
            IsActive = true,
            CreatedByUserId = userId,
            ModifiedByUserId = userId
        };

        var id = await repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<CreateEntityResponseModel>(
            OperationResult.Created,
            new CreateEntityResponseModel { Id = id });
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
