using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.TradingInstruments.Commands;

public sealed class HardDeleteTradingInstrumentCommand(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<HardDeleteEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class HardDeleteTradingInstrumentCommandValidator
        : AbstractValidator<HardDeleteTradingInstrumentCommand>
    {
        public HardDeleteTradingInstrumentCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}

public sealed class HardDeleteTradingInstrumentCommandHandler(ITradeXRepository repository)
    : IRequestHandler<HardDeleteTradingInstrumentCommand, StandardResponse<HardDeleteEntityResponseModel>>
{
    public async Task<StandardResponse<HardDeleteEntityResponseModel>> Handle(
        HardDeleteTradingInstrumentCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var entity = await repository.GetSingleAsync<TradingInstrument>(
                instrument => instrument.Id == request.Id && instrument.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<HardDeleteEntityResponseModel>(
                request.Id,
                nameof(TradingInstrument));
        }

        var relatedTradeId = await repository.GetIdAsync<Trade>(
                trade => trade.TradingInstrumentId == entity.Id,
                cancellationToken)
            .ConfigureAwait(false);

        if (relatedTradeId.HasValue)
        {
            return new StandardResponse<HardDeleteEntityResponseModel>(
                OperationResult.BadRequest,
                "Entity has related records.",
                null!);
        }

        await repository.DeleteHardAsync<TradingInstrument>(entity.Id, cancellationToken)
            .ConfigureAwait(false);

        return new StandardResponse<HardDeleteEntityResponseModel>(
            OperationResult.Deleted,
            new HardDeleteEntityResponseModel());
    }
}
