using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.TradingAccounts.Commands;

public sealed class SoftDeleteTradingAccountCommand(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<SoftDeleteEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class SoftDeleteTradingAccountCommandValidator
        : AbstractValidator<SoftDeleteTradingAccountCommand>
    {
        public SoftDeleteTradingAccountCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}

public sealed class SoftDeleteTradingAccountCommandHandler(ITradeXRepository repository)
    : IRequestHandler<SoftDeleteTradingAccountCommand, StandardResponse<SoftDeleteEntityResponseModel>>
{
    public async Task<StandardResponse<SoftDeleteEntityResponseModel>> Handle(
        SoftDeleteTradingAccountCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        var entity = await repository.GetSingleAsync<TradingAccount>(
                account => account.Id == request.Id && account.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<SoftDeleteEntityResponseModel>(
                request.Id,
                nameof(TradingAccount));
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
}
