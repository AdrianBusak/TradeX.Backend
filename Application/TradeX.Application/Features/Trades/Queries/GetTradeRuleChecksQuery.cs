using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Clients.Features.Trades;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Trades.Queries;

public sealed class GetTradeRuleChecksQuery(Guid tradeId)
    : ContextualRequest,
      IRequest<StandardResponse<GetTradeRuleChecksResponse>>,
      IAuthenticatedRequest
{
    public Guid TradeId { get; } = tradeId;
}

public sealed class GetTradeRuleChecksQueryHandler(ITradeXRepository repository)
    : IRequestHandler<GetTradeRuleChecksQuery, StandardResponse<GetTradeRuleChecksResponse>>
{
    public async Task<StandardResponse<GetTradeRuleChecksResponse>> Handle(
        GetTradeRuleChecksQuery request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var trade = await repository.GetSingleAsync<Trade>(
                entity => entity.Id == request.TradeId && entity.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (trade is null)
        {
            return new StandardResponse<GetTradeRuleChecksResponse>(
                OperationResult.NotFound,
                "Trade was not found.",
                null!);
        }

        var model = await TradeRuleCheckResponseFactory
            .CreateAsync(repository, trade, userId, cancellationToken)
            .ConfigureAwait(false);

        return new StandardResponse<GetTradeRuleChecksResponse>(OperationResult.Ok, model);
    }
}
