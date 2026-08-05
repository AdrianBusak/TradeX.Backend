using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Strategies.Commands;

public sealed class RestoreStrategyCommand(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<RestoreEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class RestoreStrategyCommandValidator
        : AbstractValidator<RestoreStrategyCommand>
    {
        public RestoreStrategyCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}

public sealed class RestoreStrategyCommandHandler(ITradeXRepository repository)
    : IRequestHandler<RestoreStrategyCommand, StandardResponse<RestoreEntityResponseModel>>
{
    public async Task<StandardResponse<RestoreEntityResponseModel>> Handle(
        RestoreStrategyCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        var entity = await repository.GetSingleAsync<Strategy>(
                strategy => strategy.Id == request.Id && strategy.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<RestoreEntityResponseModel>(
                request.Id,
                nameof(Strategy));
        }

        if (entity.IsActive)
        {
            return new StandardResponse<RestoreEntityResponseModel>(
                OperationResult.BadRequest,
                "Entity is already active.",
                null!);
        }

        entity.IsActive = true;
        entity.ModifiedByUserId = userId;

        await repository.UpdateAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        return new StandardResponse<RestoreEntityResponseModel>(
            OperationResult.Updated,
            new RestoreEntityResponseModel());
    }
}
