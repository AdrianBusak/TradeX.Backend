using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeX.Application.Clients.Features.LotCalculator.Services;
using TradeX.Infrastructure.LotCalculator.Services;

namespace TradeX.Infrastructure.LotCalculator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLotCalculatorInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IExchangeRateProvider, ExchangeRateProvider>();
        services.AddSingleton<IInstrumentSpecificationResolver, InstrumentSpecificationResolver>();
        services.AddScoped<ILotCalculatorService, LotCalculatorService>();
        return services;
    }
}
