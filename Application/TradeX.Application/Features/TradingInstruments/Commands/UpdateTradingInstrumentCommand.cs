using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;
using static TradeX.Application.Clients.Features.TradingInstruments.Commands.UpdateTradingInstrumentCommand;

namespace TradeX.Application.Clients.Features.TradingInstruments.Commands;

public sealed class UpdateTradingInstrumentCommand(Guid id, UpdateTradingInstrumentCommandModel data)
    : BaseInput<UpdateTradingInstrumentCommandModel>(data),
      IRequest<StandardResponse<UpdateEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class UpdateTradingInstrumentCommandModel
    {
        public string Symbol { get; set; } = null!;
        public MarketType MarketType { get; set; }
    }

    public sealed class Validator : AbstractValidator<UpdateTradingInstrumentCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Model).NotEmpty().SetValidator(new ModelValidator());
        }
    }

    public sealed class ModelValidator : AbstractValidator<UpdateTradingInstrumentCommandModel>
    {
        public ModelValidator()
        {
            RuleFor(x => x.Symbol).NotEmpty().MaximumLength(30);
            RuleFor(x => x.MarketType).IsInEnum();
        }
    }
}

public sealed class UpdateTradingInstrumentCommandHandler(ITradeXRepository repository)
    : IRequestHandler<UpdateTradingInstrumentCommand, StandardResponse<UpdateEntityResponseModel>>
{
    public async Task<StandardResponse<UpdateEntityResponseModel>> Handle(UpdateTradingInstrumentCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var entity = await repository.GetSingleAsync<TradingInstrument>(
                instrument => instrument.Id == request.Id && instrument.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<UpdateEntityResponseModel>(request.Id, nameof(TradingInstrument));
        }

        var symbol = request.Model.Symbol.Trim().ToUpperInvariant();
        var duplicateId = await repository.GetIdAsync<TradingInstrument>(
                instrument => instrument.UserId == userId && instrument.Id != entity.Id && instrument.Symbol.ToUpper() == symbol,
                cancellationToken)
            .ConfigureAwait(false);

        if (duplicateId.HasValue)
        {
            return new StandardResponse<UpdateEntityResponseModel>(OperationResult.Conflict, "Entity with the same symbol already exists.", null!);
        }

        entity.Symbol = symbol;
        entity.MarketType = request.Model.MarketType;
        entity.ModifiedByUserId = userId;
        await repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<UpdateEntityResponseModel>(OperationResult.Updated, new UpdateEntityResponseModel());
    }
}
