using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;
using static TradeX.Application.Clients.Features.TradingInstruments.Commands.CreateTradingInstrumentCommand;

namespace TradeX.Application.Clients.Features.TradingInstruments.Commands;

public sealed class CreateTradingInstrumentCommand(CreateTradingInstrumentCommandModel data)
    : BaseInput<CreateTradingInstrumentCommandModel>(data),
      IRequest<StandardResponse<CreateEntityResponseModel>>,
      IAuthenticatedRequest
{
    public sealed class CreateTradingInstrumentCommandModel
    {
        public string Symbol { get; set; } = null!;
        public MarketType MarketType { get; set; }
    }

    public sealed class Validator : AbstractValidator<CreateTradingInstrumentCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Model)
                .NotEmpty()
                .SetValidator(new ModelValidator());
        }
    }

    public sealed class ModelValidator : AbstractValidator<CreateTradingInstrumentCommandModel>
    {
        public ModelValidator()
        {
            RuleFor(x => x.Symbol).NotEmpty().MaximumLength(30);
            RuleFor(x => x.MarketType).IsInEnum();
        }
    }
}

public sealed class CreateTradingInstrumentCommandHandler(ITradeXRepository repository)
    : IRequestHandler<CreateTradingInstrumentCommand, StandardResponse<CreateEntityResponseModel>>
{
    public async Task<StandardResponse<CreateEntityResponseModel>> Handle(
        CreateTradingInstrumentCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var symbol = request.Model.Symbol.Trim().ToUpperInvariant();

        var duplicateId = await repository.GetIdAsync<TradingInstrument>(
            x => x.UserId == userId && x.Symbol.ToUpper() == symbol,
            cancellationToken);

        if (duplicateId.HasValue)
        {
            return new StandardResponse<CreateEntityResponseModel>(
                OperationResult.Conflict,
                "Entity with the same symbol already exists.",
                null!);
        }

        var id = await repository.AddAsync(
            new TradingInstrument
            {
                UserId = userId,
                Symbol = symbol,
                MarketType = request.Model.MarketType,
                IsActive = true,
                CreatedByUserId = userId,
                ModifiedByUserId = userId
            },
            cancellationToken);

        return new StandardResponse<CreateEntityResponseModel>(
            OperationResult.Created,
            new CreateEntityResponseModel { Id = id });
    }
}
