using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.TradingInstruments.Commands;

public sealed class SoftDeleteTradingInstrumentCommand(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<SoftDeleteEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class SoftDeleteTradingInstrumentCommandValidator
        : AbstractValidator<SoftDeleteTradingInstrumentCommand>
    {
        public SoftDeleteTradingInstrumentCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}

public sealed class SoftDeleteTradingInstrumentCommandHandler(ITradeXRepository repository)
    : IRequestHandler<SoftDeleteTradingInstrumentCommand, StandardResponse<SoftDeleteEntityResponseModel>>
{
    public async Task<StandardResponse<SoftDeleteEntityResponseModel>> Handle(
        SoftDeleteTradingInstrumentCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var entity = await repository.GetSingleAsync<TradingInstrument>(
                instrument => instrument.Id == request.Id && instrument.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<SoftDeleteEntityResponseModel>(request.Id, nameof(TradingInstrument));
        }

        if (!entity.IsActive)
        {
            return new StandardResponse<SoftDeleteEntityResponseModel>(OperationResult.BadRequest, "Entity is already deleted.", null!);
        }

        entity.IsActive = false;
        entity.ModifiedByUserId = userId;
        await repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<SoftDeleteEntityResponseModel>(OperationResult.Deleted, new SoftDeleteEntityResponseModel());
    }
}
