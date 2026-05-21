using TradeX.Infrastrucure.JwtAuthorization.Configuration;
using TradeX.Infrastrucure.JwtAuthorization.Interfaces;
using TradeX.Infrastrucure.JwtAuthorization.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TradeX.Infrastrucure.JwtAuthorization.Extensions;

public static class AddJwtAuthorizationSettings
{
    public static IServiceCollection AddJwtAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var tokenConfiguration = configuration.GetRequiredSection("TokenConfiguration").Get<TokenConfiguration>()!;
        ArgumentNullException.ThrowIfNull(tokenConfiguration);

        services.AddSingleton(tokenConfiguration);
        services.AddSingleton<IOpenIdConnectConfigurationReader, OpenIdConnectConfigurationReader>();

        services.AddScoped<ITokenValidatorService, TokenValidatorService>();

        return services;
    }
}
