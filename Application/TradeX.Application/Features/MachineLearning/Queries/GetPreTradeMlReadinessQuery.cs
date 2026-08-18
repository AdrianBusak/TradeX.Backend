using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;

namespace TradeX.Application.Clients.Features.MachineLearning.Queries;

public sealed class GetPreTradeMlReadinessQuery
    : ContextualRequest,
      IRequest<StandardResponse<PreTradeMlReadinessResponse>>,
      IAuthenticatedRequest;

public sealed class GetPreTradeMlReadinessQueryHandler(ITradeOutcomeMlService service)
    : IRequestHandler<GetPreTradeMlReadinessQuery, StandardResponse<PreTradeMlReadinessResponse>>
{
    public async Task<StandardResponse<PreTradeMlReadinessResponse>> Handle(
        GetPreTradeMlReadinessQuery request,
        CancellationToken cancellationToken)
    {
        var response = await service.GetReadinessAsync(request.UserId(), cancellationToken)
            .ConfigureAwait(false);

        return new StandardResponse<PreTradeMlReadinessResponse>(OperationResult.Ok, response);
    }
}
