using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.TradingInstruments.Queries;

public sealed class GetTradingInstrumentByIdQuery(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<GetTradingInstrumentByIdResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class GetTradingInstrumentByIdQueryValidator
        : AbstractValidator<GetTradingInstrumentByIdQuery>
    {
        public GetTradingInstrumentByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}

public sealed class GetTradingInstrumentByIdQueryHandler(ITradeXRepository repository)
    : IRequestHandler<GetTradingInstrumentByIdQuery, StandardResponse<GetTradingInstrumentByIdResponseModel>>
{
    public async Task<StandardResponse<GetTradingInstrumentByIdResponseModel>> Handle(GetTradingInstrumentByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var entity = await repository.GetSingleAsync<TradingInstrument>(
                instrument => instrument.Id == request.Id && instrument.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<GetTradingInstrumentByIdResponseModel>(request.Id, nameof(TradingInstrument));
        }

        return new StandardResponse<GetTradingInstrumentByIdResponseModel>(
            OperationResult.Ok,
            new GetTradingInstrumentByIdResponseModel
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Symbol = entity.Symbol,
                MarketType = entity.MarketType,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            });
    }
}

public sealed class GetTradingInstrumentByIdResponseModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Symbol { get; set; } = null!;
    public MarketType MarketType { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}
