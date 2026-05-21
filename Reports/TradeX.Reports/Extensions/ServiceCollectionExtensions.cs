using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TradeX.Reports.Encounter;

namespace TradeX.Reports.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReports(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        
        //services.AddScoped<EncounterReportGenerator>();
        services.AddScoped<IEncounterReportGenerator, EncounterReportGenerator>();

        return services;
    }
}