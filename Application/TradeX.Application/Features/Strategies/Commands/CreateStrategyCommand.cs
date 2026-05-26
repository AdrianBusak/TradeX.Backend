using FluentValidation;
using MediatR;
using System.Text.RegularExpressions;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;
using static TradeX.Application.Clients.Features.Strategies.Commands.CreateStrategyCommand;

namespace TradeX.Application.Clients.Features.Strategies.Commands;

public sealed class CreateStrategyCommand(CreateStrategyCommandModel data)
    : BaseInput<CreateStrategyCommandModel>(data),
      IRequest<StandardResponse<CreateEntityResponseModel>>,
      IAuthenticatedRequest
{
    public sealed class CreateStrategyCommandModel
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public MarketType MarketType { get; set; }
        public string? Color { get; set; }
    }

    public sealed class CreateStrategyCommandValidator : AbstractValidator<CreateStrategyCommand>
    {
        public CreateStrategyCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotEmpty()
                .SetValidator(new CreateStrategyCommandModelValidator());
        }
    }

    public sealed class CreateStrategyCommandModelValidator : AbstractValidator<CreateStrategyCommandModel>
    {
        public CreateStrategyCommandModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(2000);

            RuleFor(x => x.MarketType)
                .IsInEnum();

            RuleFor(x => x.Color)
                .Must(BeHexColor)
                .When(x => !string.IsNullOrWhiteSpace(x.Color))
                .WithMessage("Color must be a hex color in #RRGGBB format.");
        }

        private static bool BeHexColor(string? color)
        {
            var normalizedColor = string.IsNullOrWhiteSpace(color) ? null : color.Trim();

            return normalizedColor is null ||
                   Regex.IsMatch(normalizedColor, "^#[0-9A-Fa-f]{6}$");
        }
    }
}

public sealed class CreateStrategyCommandHandler(ITradeXRepository repository)
    : IRequestHandler<CreateStrategyCommand, StandardResponse<CreateEntityResponseModel>>
{
    public async Task<StandardResponse<CreateEntityResponseModel>> Handle(
        CreateStrategyCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var name = NormalizeRequired(request.Model.Name);

        if (await NameExistsAsync(userId, name, cancellationToken).ConfigureAwait(false))
        {
            return new StandardResponse<CreateEntityResponseModel>(
                OperationResult.Conflict,
                "Entity with the same name already exists.",
                null!);
        }

        var entity = new Strategy
        {
            UserId = userId,
            Name = name,
            Description = NormalizeOptional(request.Model.Description),
            MarketType = request.Model.MarketType,
            Color = NormalizeOptional(request.Model.Color),
            IsActive = true,
            CreatedByUserId = userId,
            ModifiedByUserId = userId
        };

        var id = await repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<CreateEntityResponseModel>(
            OperationResult.Created,
            new CreateEntityResponseModel { Id = id });
    }

    private async Task<bool> NameExistsAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.ToUpperInvariant();

        var existingId = await repository.GetIdAsync<Strategy>(
                strategy => strategy.UserId == userId &&
                            strategy.Name.ToUpper() == normalizedName,
                cancellationToken)
            .ConfigureAwait(false);

        return existingId.HasValue;
    }

    private static string NormalizeRequired(string value)
        => value.Trim();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
