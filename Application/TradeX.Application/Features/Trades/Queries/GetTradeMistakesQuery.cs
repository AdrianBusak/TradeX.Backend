using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Clients.Features.Trades;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Trades.Queries;

public sealed class GetTradeMistakesQuery(Guid tradeId)
    : ContextualRequest,
      IRequest<StandardResponse<GetTradeMistakesResponse>>,
      IAuthenticatedRequest
{
    public Guid TradeId { get; } = tradeId;

    public sealed class Validator : AbstractValidator<GetTradeMistakesQuery>
    {
        public Validator() => RuleFor(x => x.TradeId).NotEmpty();
    }
}

public sealed class GetTradeMistakesQueryHandler(ITradeXRepository repository)
    : IRequestHandler<GetTradeMistakesQuery, StandardResponse<GetTradeMistakesResponse>>
{
    public async Task<StandardResponse<GetTradeMistakesResponse>> Handle(
        GetTradeMistakesQuery request,
        CancellationToken cancellationToken)
    {
        var trade = await repository.GetSingleAsync<Trade>(
                entity => entity.Id == request.TradeId && entity.UserId == request.UserId(),
                cancellationToken)
            .ConfigureAwait(false);

        if (trade is null)
        {
            return new StandardResponse<GetTradeMistakesResponse>(
                OperationResult.NotFound,
                "Trade was not found.",
                null!);
        }

        var model = await TradeMistakeResponseFactory
            .CreateAsync(repository, trade, cancellationToken)
            .ConfigureAwait(false);

        return new StandardResponse<GetTradeMistakesResponse>(OperationResult.Ok, model);
    }
}
