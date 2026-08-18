using API.Abstractions.Interfaces;
using API.Abstractions.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TradeX.Application.Abstractions.Configuration;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Clients.Extensions;
using TradeX.Infrastructure.BlobStorageClient.Extensions;
using TradeX.Infrastrucure.JwtAuthorization.Extensions;
using TradeX.Infrastructure.EconomicCalendar.Extensions;
using TradeX.Infrastructure.LotCalculator.Extensions;
using TradeX.Infrastructure.MachineLearning.Extensions;
using TradeX.Repository;
using TradeX.API.Services;

namespace TradeX.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddJwtAuthorization(configuration);
        services.AddClientsApplication(configuration);
        services.AddScoped<IUserContextAccessor, UserContextAccessor>();
        services.AddScoped<IHttpRequestProcessingService, HttpRequestProcessingService>();
        services.AddBlobStorage(configuration);
        services.AddLotCalculatorInfrastructure(configuration);
        services.AddEconomicCalendarInfrastructure(configuration);
        services.AddHostedService<EconomicCalendarSyncWorker>();
        services.AddMachineLearningInfrastructure(configuration);

        services.AddSingleton(configuration.GetRequiredSection("ApplicationConfiguration").Get<ApplicationConfiguration>()!);

        return services;
    }

    public static IServiceCollection ExecuteMigrations(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();

        var ctx = sp.GetRequiredService<TradeXDbContext>();
        var log = sp.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Migration");

        try
        {
            ctx.Database.Migrate();

            log.LogInformation("Migrations executed");
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to execute migrations.");

            if (Debugger.IsAttached)
            {
                throw;
            }
        }

        return services;
    }
}
