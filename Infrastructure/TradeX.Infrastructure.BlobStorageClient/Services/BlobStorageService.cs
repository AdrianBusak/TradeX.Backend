using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Infrastructure.BlobStorageClient.Configuration;

namespace TradeX.Infrastructure.BlobStorageClient.Services;

public class BlobStorageService(BlobServiceClient blobServiceClient, BlobStorageClientConfiguration config) : IBlobStorageService
{
    private readonly BlobStorageClientConfiguration _config = config;
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;

    public async Task<string> UploadAsync(Stream fileStream, string blobName, string contentType, CancellationToken cancellationToken)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_config.ContainerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                Metadata = new Dictionary<string, string> { { "UploadedBy", "TradeXAPI" } }
            }, cancellationToken);

            return blobName;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Azure Blob upload failed.", ex);
        }
    }

    public async Task DeleteAsync(string blobPath, CancellationToken cancellationToken)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_config.ContainerName);
            await containerClient.GetBlobClient(blobPath)
                .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Azure Blob delete failed.", ex);
        }
    }

    public string GetSasUrl(string blobPath, int expiryMinutes = 60)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobPath);
        var containerClient = _blobServiceClient.GetBlobContainerClient(_config.ContainerName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException("Blob Storage connection string must include AccountKey to generate SAS URLs.");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _config.ContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }
}