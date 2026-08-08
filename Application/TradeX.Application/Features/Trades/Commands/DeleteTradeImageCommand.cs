using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Trades.Commands;

public sealed class DeleteTradeImageCommand(Guid tradeId, Guid imageId)
    : ContextualRequest,
      IRequest<StandardResponse<HardDeleteEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid TradeId { get; } = tradeId;
    public Guid ImageId { get; } = imageId;
}

public sealed class DeleteTradeImageCommandHandler(
    ITradeXRepository repository,
    IBlobStorageService blobStorage)
    : IRequestHandler<DeleteTradeImageCommand, StandardResponse<HardDeleteEntityResponseModel>>
{
    public async Task<StandardResponse<HardDeleteEntityResponseModel>> Handle(DeleteTradeImageCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var image = await repository.GetSingleAsync<TradeImage>(
            x => x.Id == request.ImageId && x.TradeId == request.TradeId && x.UserId == userId,
            cancellationToken);

        if (image is null)
        {
            return new StandardResponse<HardDeleteEntityResponseModel>(OperationResult.NotFound, "Trade image was not found.", null!);
        }

        await blobStorage.DeleteAsync(image.BlobPath, cancellationToken);
        await repository.DeleteHardAsync<TradeImage>(image.Id, cancellationToken);

        return new StandardResponse<HardDeleteEntityResponseModel>(OperationResult.Deleted, new HardDeleteEntityResponseModel());
    }
}
