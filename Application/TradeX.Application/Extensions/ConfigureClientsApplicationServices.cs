using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TradeX.Application.Abstractions.Extensions;

namespace TradeX.Application.Clients.Extensions;

public static class ConfigureClientsApplicationServices
{
    public static IServiceCollection AddClientsApplication(this IServiceCollection services, IConfiguration configuration, Action<IServiceCollection>? mediatrAddingBehaviors = null, Action<IServiceCollection>? mediatrAddedBehaviors = null)
    {
        var applicationAssembly = Assembly.GetExecutingAssembly();

        services.AddApplication(configuration, applicationAssembly, mediatrAddingBehaviors, mediatrAddedBehaviors);

        return services;
    }
}
