using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.TradingInstruments.Commands;

public sealed class RestoreTradingInstrumentCommand(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<RestoreEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class RestoreTradingInstrumentCommandValidator
        : AbstractValidator<RestoreTradingInstrumentCommand>
    {
        public RestoreTradingInstrumentCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}

public sealed class RestoreTradingInstrumentCommandHandler(ITradeXRepository repository)
    : IRequestHandler<RestoreTradingInstrumentCommand, StandardResponse<RestoreEntityResponseModel>>
{
    public async Task<StandardResponse<RestoreEntityResponseModel>> Handle(
        RestoreTradingInstrumentCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var entity = await repository.GetSingleAsync<TradingInstrument>(
                instrument => instrument.Id == request.Id && instrument.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<RestoreEntityResponseModel>(request.Id, nameof(TradingInstrument));
        }

        if (entity.IsActive)
        {
            return new StandardResponse<RestoreEntityResponseModel>(OperationResult.BadRequest, "Entity is already active.", null!);
        }

        entity.IsActive = true;
        entity.ModifiedByUserId = userId;
        await repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<RestoreEntityResponseModel>(OperationResult.Updated, new RestoreEntityResponseModel());
    }
}
