using FluentValidation;
using MediatR;
using System.Text.RegularExpressions;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;
using static TradeX.Application.Clients.Features.Strategies.Commands.UpdateStrategyCommand;

namespace TradeX.Application.Clients.Features.Strategies.Commands;

public sealed class UpdateStrategyCommand(Guid id, UpdateStrategyCommandModel data)
    : BaseInput<UpdateStrategyCommandModel>(data),
      IRequest<StandardResponse<UpdateEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class UpdateStrategyCommandModel
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public MarketType MarketType { get; set; }
        public string? Color { get; set; }
    }

    public sealed class UpdateStrategyCommandValidator : AbstractValidator<UpdateStrategyCommand>
    {
        public UpdateStrategyCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Model)
                .NotEmpty()
                .SetValidator(new UpdateStrategyCommandModelValidator());
        }
    }

    public sealed class UpdateStrategyCommandModelValidator : AbstractValidator<UpdateStrategyCommandModel>
    {
        public UpdateStrategyCommandModelValidator()
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

public sealed class UpdateStrategyCommandHandler(ITradeXRepository repository)
    : IRequestHandler<UpdateStrategyCommand, StandardResponse<UpdateEntityResponseModel>>
{
    public async Task<StandardResponse<UpdateEntityResponseModel>> Handle(
        UpdateStrategyCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        var entity = await repository.GetSingleAsync<Strategy>(
                strategy => strategy.Id == request.Id && strategy.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<UpdateEntityResponseModel>(
                request.Id,
                nameof(Strategy));
        }

        var name = NormalizeRequired(request.Model.Name);

        if (await NameExistsAsync(userId, entity.Id, name, cancellationToken).ConfigureAwait(false))
        {
            return new StandardResponse<UpdateEntityResponseModel>(
                OperationResult.Conflict,
                "Entity with the same name already exists.",
                null!);
        }

        entity.Name = name;
        entity.Description = NormalizeOptional(request.Model.Description);
        entity.MarketType = request.Model.MarketType;
        entity.Color = NormalizeOptional(request.Model.Color);
        entity.ModifiedByUserId = userId;

        await repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<UpdateEntityResponseModel>(
            OperationResult.Updated,
            new UpdateEntityResponseModel());
    }

    private async Task<bool> NameExistsAsync(
        Guid userId,
        Guid currentStrategyId,
        string name,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.ToUpperInvariant();

        var existingId = await repository.GetIdAsync<Strategy>(
                strategy => strategy.UserId == userId &&
                            strategy.Id != currentStrategyId &&
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
