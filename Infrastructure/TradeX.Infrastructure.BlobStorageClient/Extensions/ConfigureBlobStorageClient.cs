using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Infrastructure.BlobStorageClient.Configuration;
using TradeX.Infrastructure.BlobStorageClient.Services;

namespace TradeX.Infrastructure.BlobStorageClient.Extensions;

public static class ConfigureBlobStorageClient
{
    public static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetRequiredSection("BlobStorageClientConfiguration").Get<BlobStorageClientConfiguration>()!;
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(config.ConnectionString);
        ArgumentNullException.ThrowIfNull(config.ContainerName);

        services.AddSingleton(config);
        services.AddSingleton(new BlobServiceClient(config.ConnectionString));
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        return services;
    }
}
