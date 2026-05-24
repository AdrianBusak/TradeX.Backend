using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.TradingAccounts.Commands;

public sealed class RestoreTradingAccountCommand(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<RestoreEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class RestoreTradingAccountCommandValidator
        : AbstractValidator<RestoreTradingAccountCommand>
    {
        public RestoreTradingAccountCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}

public sealed class RestoreTradingAccountCommandHandler(ITradeXRepository repository)
    : IRequestHandler<RestoreTradingAccountCommand, StandardResponse<RestoreEntityResponseModel>>
{
    public async Task<StandardResponse<RestoreEntityResponseModel>> Handle(
        RestoreTradingAccountCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        var entity = await repository.GetSingleAsync<TradingAccount>(
                account => account.Id == request.Id && account.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<RestoreEntityResponseModel>(
                request.Id,
                nameof(TradingAccount));
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

        await repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<RestoreEntityResponseModel>(
            OperationResult.Updated,
            new RestoreEntityResponseModel());
    }
}
