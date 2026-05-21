using Microsoft.Extensions.Configuration;

namespace TradeX.Application.Clients.Tests.Configuration;

public static class Helpers
{
    public static IConfiguration InitConfiguration()
    {
        var config = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();

        return config;
    }
}