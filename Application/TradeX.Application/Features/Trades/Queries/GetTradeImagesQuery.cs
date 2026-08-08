using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Clients.Features.Trades;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Trades.Queries;

public sealed class GetTradeImagesQuery(Guid tradeId)
    : ContextualRequest,
      IRequest<StandardResponse<List<TradeImageResponseModel>>>,
      IAuthenticatedRequest
{
    public Guid TradeId { get; } = tradeId;
}

public sealed class GetTradeImagesQueryHandler(ITradeXRepository repository, IBlobStorageService blobStorage)
    : IRequestHandler<GetTradeImagesQuery, StandardResponse<List<TradeImageResponseModel>>>
{
    public async Task<StandardResponse<List<TradeImageResponseModel>>> Handle(GetTradeImagesQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var images = await repository.GetListAsync<TradeImage, DateTimeOffset>(
            x => x.CreatedAt,
            null,
            -1,
            cancellationToken,
            x => x.TradeId == request.TradeId && x.UserId == userId && x.IsActive);

        var model = images.Select(image => new TradeImageResponseModel
        {
            Id = image.Id,
            OriginalFileName = image.OriginalFileName,
            ContentType = image.ContentType,
            SizeBytes = image.SizeBytes,
            UploadedAt = image.CreatedAt,
            Url = blobStorage.GetSasUrl(image.BlobPath)
        }).ToList();

        return new StandardResponse<List<TradeImageResponseModel>>(OperationResult.Ok, model);
    }
}
