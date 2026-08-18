using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;

namespace TradeX.Application.Clients.Features.MachineLearning.Commands;

public sealed class TrainPreTradeMlModelCommand
    : ContextualRequest,
      IRequest<StandardResponse<TrainPreTradeMlModelResponse>>,
      IAuthenticatedRequest;

public sealed class TrainPreTradeMlModelCommandHandler(ITradeOutcomeMlService service)
    : IRequestHandler<TrainPreTradeMlModelCommand, StandardResponse<TrainPreTradeMlModelResponse>>
{
    public async Task<StandardResponse<TrainPreTradeMlModelResponse>> Handle(
        TrainPreTradeMlModelCommand request,
        CancellationToken cancellationToken)
    {
        var response = await service.TrainAsync(request.UserId(), cancellationToken)
            .ConfigureAwait(false);

        return new StandardResponse<TrainPreTradeMlModelResponse>(
            response.IsReady ? OperationResult.Created : OperationResult.BadRequest,
            response);
    }
}
