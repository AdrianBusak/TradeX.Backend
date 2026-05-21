using TradeX.Application.Abstractions.Configuration;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Clients.Extensions;
using TradeX.Application.Clients.Tests.Configuration;
using TradeX.Application.Clients.Tests.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TestHelper = TradeX.Application.Clients.Tests.Configuration.Helpers;

namespace TradeX.Application.Clients.Tests.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestServices(this IServiceCollection services)
    {
        var config = TestHelper.InitConfiguration();

        var testProjectConfigurationSection = config.GetSection("TestProjectConfiguration");

        services.Configure<TestProjectConfiguration>(testProjectConfigurationSection);
        services.AddScoped(cfg => cfg.GetRequiredService<IOptions<TestProjectConfiguration>>().Value);

        services.Configure<ApplicationConfiguration>(config.GetSection("ApplicationConfiguration"));
        services.AddSingleton(cfg => cfg.GetRequiredService<IOptions<ApplicationConfiguration>>().Value);

        var provider = services.BuildServiceProvider();

        var testProjectConfig = provider.GetRequiredService<TestProjectConfiguration>();

        services.AddClientsApplication(config);
        services.AddSingleton<IUserContextAccessor, UserContextAccessor>();
        
        var sp = services.BuildServiceProvider();

        var testProjectConfiguration = sp.GetService<TestProjectConfiguration>();
        var applicationConfiguration = sp.GetService<ApplicationConfiguration>();

        services.AddLogging(config => config.AddDebug());

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings { MaxDepth = 128 };

        return services;
    }
}
