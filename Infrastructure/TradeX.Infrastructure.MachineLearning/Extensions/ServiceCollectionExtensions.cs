using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeX.Application.Clients.Features.MachineLearning;
using TradeX.Infrastructure.MachineLearning.Configuration;

namespace TradeX.Infrastructure.MachineLearning.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMachineLearningInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var modelStorageConfiguration = configuration
            .GetRequiredSection("TradeOutcomeModelStorage")
            .Get<TradeOutcomeModelStorageConfiguration>()!;

        ArgumentException.ThrowIfNullOrWhiteSpace(modelStorageConfiguration.ContainerName);

        services.AddSingleton(modelStorageConfiguration);
        services.AddSingleton(new Microsoft.ML.MLContext(seed: 42));
        services.AddScoped<TradeOutcomeFeatureBuilder>();
        services.AddScoped<ITradeOutcomeDatasetBuilder, TradeOutcomeDatasetBuilder>();
        services.AddScoped<ITradeOutcomeModelStorage, AzureBlobTradeOutcomeModelStorage>();
        services.AddScoped<ITradeOutcomeMlService, TradeOutcomeMlService>();

        return services;
    }
}
