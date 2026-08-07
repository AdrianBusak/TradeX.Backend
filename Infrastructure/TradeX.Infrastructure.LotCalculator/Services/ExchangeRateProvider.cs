using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using TradeX.Application.Clients.Features.LotCalculator.Services;

namespace TradeX.Infrastructure.LotCalculator.Services;

public sealed class ExchangeRateProvider(IMemoryCache cache, IConfiguration configuration)
    : IExchangeRateProvider
{
    private static readonly HttpClient HttpClient = new();

    public async Task<decimal> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        var from = NormalizeCurrency(fromCurrency);
        var to = NormalizeCurrency(toCurrency);
        if (from == to)
        {
            return 1m;
        }

        var cacheKey = $"lot-calculator-rate:{from}:{to}";
        if (cache.TryGetValue(cacheKey, out decimal cachedRate))
        {
            return cachedRate;
        }

        var apiKey = configuration["LotCalculator:ExchangeRates:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Exchange-rate API key is not configured on the server.");
        }

        var baseUrl = configuration["LotCalculator:ExchangeRates:BaseUrl"]
            ?? "https://v6.exchangerate-api.com/v6";
        using var response = await HttpClient.GetAsync(
            $"{baseUrl.TrimEnd('/')}/{Uri.EscapeDataString(apiKey)}/pair/{from}/{to}", cancellationToken)
            .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Exchange-rate provider could not return a conversion rate.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (!document.RootElement.TryGetProperty("conversion_result", out var element) ||
            !element.TryGetDecimal(out var rate) || rate <= 0)
        {
            throw new InvalidOperationException("Exchange-rate provider returned an invalid conversion rate.");
        }

        cache.Set(cacheKey, rate, TimeSpan.FromMinutes(45));
        return rate;
    }

    private static string NormalizeCurrency(string currency)
    {
        var normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3 || !normalized.All(char.IsLetter))
        {
            throw new ArgumentException("Currency must be a three-letter ISO currency code.");
        }

        return normalized;
    }
}
