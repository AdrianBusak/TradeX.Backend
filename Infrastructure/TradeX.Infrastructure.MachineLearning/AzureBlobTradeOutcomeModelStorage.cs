using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using TradeX.Infrastructure.MachineLearning.Configuration;

namespace TradeX.Infrastructure.MachineLearning;

internal sealed class AzureBlobTradeOutcomeModelStorage(
    BlobServiceClient blobServiceClient,
    TradeOutcomeModelStorageConfiguration configuration,
    MLContext mlContext,
    ILogger<AzureBlobTradeOutcomeModelStorage> logger)
    : ITradeOutcomeModelStorage
{
    public async Task<string> SaveAsync(
        Guid userId,
        string modelVersion,
        ITransformer model,
        DataViewSchema schema,
        CancellationToken cancellationToken)
    {
        var blobPath = GetBlobPath(userId, modelVersion);

        try
        {
            var container = blobServiceClient.GetBlobContainerClient(configuration.ContainerName);
            await container.CreateIfNotExistsAsync(
                    PublicAccessType.None,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            await using var modelStream = new MemoryStream();
            mlContext.Model.Save(model, schema, modelStream);
            modelStream.Position = 0;

            await container.GetBlobClient(blobPath)
                .UploadAsync(
                    modelStream,
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = "application/octet-stream" },
                        Metadata = new Dictionary<string, string>
                        {
                            ["UserId"] = userId.ToString("N"),
                            ["ModelVersion"] = modelVersion,
                            ["Storage"] = "TradeXMachineLearning"
                        }
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            logger.LogInformation("Saved ML model {ModelVersion} for user {UserId} to Azure Blob Storage.", modelVersion, userId);
            return blobPath;
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Failed to save ML model {ModelVersion} for user {UserId} to Azure Blob Storage.", modelVersion, userId);
            throw new InvalidOperationException("ML model upload to Azure Blob Storage failed.", ex);
        }
    }

    public async Task<ITransformer> LoadAsync(string modelPath, CancellationToken cancellationToken)
    {
        try
        {
            var container = blobServiceClient.GetBlobContainerClient(configuration.ContainerName);
            var response = await container.GetBlobClient(modelPath)
                .DownloadStreamingAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            await using var source = response.Value.Content;
            await using var modelStream = new MemoryStream();
            await source.CopyToAsync(modelStream, cancellationToken).ConfigureAwait(false);
            modelStream.Position = 0;

            return mlContext.Model.Load(modelStream, out _);
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Failed to load ML model from Azure Blob path {ModelPath}.", modelPath);
            throw new InvalidOperationException("ML model download from Azure Blob Storage failed.", ex);
        }
    }

    private static string GetBlobPath(Guid userId, string modelVersion)
        => $"trade-outcome-models/{userId:N}/{modelVersion}.zip";
}
