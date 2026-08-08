using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using TradeX.Application.Clients.Features.Trades;

namespace TradeX.Application.Clients.Features.Trades.Commands;

public sealed class UploadTradeImageCommand(Guid tradeId, Stream fileStream, string fileName, long sizeBytes)
    : ContextualRequest,
      IRequest<StandardResponse<TradeImageResponseModel>>,
      IAuthenticatedRequest
{
    public Guid TradeId { get; } = tradeId;
    public Stream FileStream { get; } = fileStream;
    public string FileName { get; } = fileName;
    public long SizeBytes { get; } = sizeBytes;
}

public sealed class UploadTradeImageCommandHandler(
    ITradeXRepository repository,
    IBlobStorageService blobStorage)
    : IRequestHandler<UploadTradeImageCommand, StandardResponse<TradeImageResponseModel>>
{
    private const long MaxImageSizeBytes = 25 * 1024 * 1024;

    public async Task<StandardResponse<TradeImageResponseModel>> Handle(UploadTradeImageCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var trade = await repository.GetSingleAsync<Trade>(x => x.Id == request.TradeId && x.UserId == userId, cancellationToken);
        if (trade is null)
        {
            return new StandardResponse<TradeImageResponseModel>(OperationResult.NotFound, "Trade was not found.", null!);
        }

        if (request.SizeBytes is <= 0 or > MaxImageSizeBytes || !request.FileStream.CanSeek)
        {
            return new StandardResponse<TradeImageResponseModel>(OperationResult.BadRequest, "Image must be non-empty, seekable, and no larger than 25 MiB.", null!);
        }

        var imageFormat = await DetectImageFormatAsync(request.FileStream, cancellationToken);
        if (imageFormat is null)
        {
            return new StandardResponse<TradeImageResponseModel>(OperationResult.BadRequest, "Only JPEG, PNG, GIF, and WebP images are allowed.", null!);
        }

        var originalFileName = Path.GetFileName(request.FileName).Trim();
        if (string.IsNullOrWhiteSpace(originalFileName) || originalFileName.Length > 255)
        {
            return new StandardResponse<TradeImageResponseModel>(OperationResult.BadRequest, "Image file name is invalid.", null!);
        }

        var blobPath = $"trades/{trade.Id:N}/{Guid.NewGuid():N}{imageFormat.Extension}";
        request.FileStream.Position = 0;
        var uploadedPath = await blobStorage.UploadAsync(request.FileStream, blobPath, imageFormat.ContentType, cancellationToken);

        var image = new TradeImage
        {
            TradeId = trade.Id,
            UserId = userId,
            BlobPath = uploadedPath,
            OriginalFileName = originalFileName,
            ContentType = imageFormat.ContentType,
            SizeBytes = request.SizeBytes,
            IsActive = true,
            CreatedByUserId = userId,
            ModifiedByUserId = userId
        };

        await repository.AddAsync(image, cancellationToken);

        return new StandardResponse<TradeImageResponseModel>(OperationResult.Created, new TradeImageResponseModel
        {
            Id = image.Id,
            OriginalFileName = image.OriginalFileName,
            ContentType = image.ContentType,
            SizeBytes = image.SizeBytes,
            UploadedAt = image.CreatedAt,
            Url = blobStorage.GetSasUrl(image.BlobPath)
        });
    }

    private static async Task<ImageFormat?> DetectImageFormatAsync(Stream stream, CancellationToken cancellationToken)
    {
        var header = new byte[12];
        stream.Position = 0;
        var read = await stream.ReadAsync(header.AsMemory(), cancellationToken);
        stream.Position = 0;

        if (read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return new(".jpg", "image/jpeg");
        if (read >= 8 && header.AsSpan()[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return new(".png", "image/png");
        if (read >= 6 && (header.AsSpan()[..6].SequenceEqual("GIF87a"u8) || header.AsSpan()[..6].SequenceEqual("GIF89a"u8))) return new(".gif", "image/gif");
        if (read >= 12 && header.AsSpan()[..4].SequenceEqual("RIFF"u8) && header.AsSpan()[8..12].SequenceEqual("WEBP"u8)) return new(".webp", "image/webp");
        return null;
    }

    private sealed record ImageFormat(string Extension, string ContentType);
}
