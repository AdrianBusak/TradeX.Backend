using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeX.Application.Clients.Features.EconomicCalendar.Services;
using TradeX.Infrastructure.EconomicCalendar.Configuration;
using TradeX.Infrastructure.EconomicCalendar.Services;

namespace TradeX.Infrastructure.EconomicCalendar.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEconomicCalendarInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetRequiredSection("EconomicCalendar").Get<EconomicCalendarConfiguration>()!;
        services.AddSingleton(settings);
        services.AddHttpClient<IEconomicCalendarProvider, ForexFactoryEconomicCalendarProvider>(
            client => ForexFactoryEconomicCalendarProvider.ConfigureHttpClient(client, settings));
        services.AddScoped<IEconomicCalendarSynchronizationService, EconomicCalendarSynchronizationService>();
        return services;
    }
}
