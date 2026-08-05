using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Trades.Commands;

public sealed class RestoreTradeCommand(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<RestoreEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class RestoreTradeCommandValidator
        : AbstractValidator<RestoreTradeCommand>
    {
        public RestoreTradeCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}

public sealed class RestoreTradeCommandHandler(ITradeXRepository repository)
    : IRequestHandler<RestoreTradeCommand, StandardResponse<RestoreEntityResponseModel>>
{
    public async Task<StandardResponse<RestoreEntityResponseModel>> Handle(
        RestoreTradeCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        var entity = await repository.GetSingleAsync<Trade>(
                trade => trade.Id == request.Id && trade.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<RestoreEntityResponseModel>(
                request.Id,
                nameof(Trade));
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
