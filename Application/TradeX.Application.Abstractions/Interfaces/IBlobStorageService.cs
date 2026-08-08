namespace TradeX.Application.Abstractions.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream fileStream, string blobName, string contentType, CancellationToken cancellationToken);
    Task DeleteAsync(string blobPath, CancellationToken cancellationToken);
    string GetSasUrl(string blobPath, int expiryMinutes = 60);
}
