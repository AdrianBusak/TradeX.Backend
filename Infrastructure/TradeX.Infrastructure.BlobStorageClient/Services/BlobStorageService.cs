using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using TradeX.Infrastructure.BlobStorageClient.Configuration;
using TradeX.Infrastructure.BlobStorageClient.Interfaces;

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

            // Osiguraj container (u praksi se ovo često radi samo jednom pri deployu/startup-u)
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient(blobName);

            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                // Opcionalno: dodajemo metapodatke ako zatrebaju za brzu pretragu bez baze
                Metadata = new Dictionary<string, string> { { "UploadedBy", "TradeXAPI" } }
            };

            await blobClient.UploadAsync(fileStream, options, cancellationToken);

            return blobName;
        }
        catch (RequestFailedException ex)
        {
            // Ovdje bi išao tvoj Logger
            throw new Exception($"Azure Blob Upload failed: {ex.Message}", ex);
        }
    }

    public async Task DeleteAsync(string blobPath, CancellationToken cancellationToken)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_config.ContainerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            // Briše blob i sve njegove snapshot-ove
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            throw new Exception($"Azure Blob Delete failed: {ex.Message}", ex);
        }
    }

    public string GetSasUrl(string blobPath, int expiryMinutes = 60)
    {
        if (string.IsNullOrWhiteSpace(blobPath)) return string.Empty;

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_config.ContainerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            // Provjera ima li klijent prava za generiranje SAS-a (Shared Key auth)
            if (!blobClient.CanGenerateSasUri)
            {
                throw new InvalidOperationException("Napomena: ConnectionString mora sadržavati AccountKey za generiranje SAS URL-ova.");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _config.ContainerName,
                BlobName = blobClient.Name,
                Resource = "b", // b = blob
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
            };

            // Postavljamo samo READ permisiju za sigurnost
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }
        catch (Exception)
        {
            // U slučaju greške vraćamo prazno kako UI ne bi pukao, 
            // ali bi bilo dobro logirati incident
            return string.Empty;
        }
    }
}