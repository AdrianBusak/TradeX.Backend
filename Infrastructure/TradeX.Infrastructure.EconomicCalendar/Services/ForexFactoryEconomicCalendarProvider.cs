using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TradeX.Application.Clients.Features.EconomicCalendar.Services;
using TradeX.Domain.Enums;
using TradeX.Infrastructure.EconomicCalendar.Configuration;

namespace TradeX.Infrastructure.EconomicCalendar.Services;

public sealed class ForexFactoryEconomicCalendarProvider(
    HttpClient httpClient,
    ILogger<ForexFactoryEconomicCalendarProvider> logger)
    : IEconomicCalendarProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<EconomicCalendarFetchResult> GetCurrentWeekAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync("", cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Economic Calendar provider returned HTTP {StatusCode}.", (int)response.StatusCode);
                return EconomicCalendarFetchResult.Failed($"Provider returned HTTP {(int)response.StatusCode} ({response.StatusCode}).");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var feed = await JsonSerializer.DeserializeAsync<List<ForexFactoryEconomicEventDto>>(
                    stream, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);
            var events = feed?.Select(TryMap).Where(x => x is not null).Cast<EconomicCalendarSourceEvent>().ToList();

            if (events is null || events.Count == 0)
            {
                logger.LogWarning("Economic Calendar provider returned an empty or invalid feed.");
                return EconomicCalendarFetchResult.Failed("Provider returned an empty or invalid feed.");
            }

            return EconomicCalendarFetchResult.Succeeded(events);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Economic Calendar provider request timed out.");
            return EconomicCalendarFetchResult.Failed("Provider request timed out.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Economic Calendar provider HTTP request failed.");
            return EconomicCalendarFetchResult.Failed("Provider HTTP request failed.");
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Economic Calendar provider returned invalid JSON.");
            return EconomicCalendarFetchResult.Failed("Provider returned invalid JSON.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Economic Calendar provider request failed unexpectedly.");
            return EconomicCalendarFetchResult.Failed("Provider request failed unexpectedly.");
        }
    }

    public static EconomicCalendarSourceEvent? TryMap(ForexFactoryEconomicEventDto source)
    {
        if (string.IsNullOrWhiteSpace(source.Title) ||
            string.IsNullOrWhiteSpace(source.Country) ||
            !DateTimeOffset.TryParse(source.Date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var scheduledAt))
        {
            return null;
        }

        return new EconomicCalendarSourceEvent(
            source.Title.Trim(),
            source.Country.Trim().ToUpperInvariant(),
            scheduledAt,
            ParseImpact(source.Impact),
            NormalizeOptional(source.Forecast),
            NormalizeOptional(source.Previous));
    }

    public static EconomicImpact ParseImpact(string? impact)
        => Enum.TryParse<EconomicImpact>(impact?.Trim(), true, out var value)
            ? value
            : EconomicImpact.Unknown;

    public static void ConfigureHttpClient(HttpClient client, EconomicCalendarConfiguration configuration)
    {
        client.BaseAddress = new Uri(configuration.Url, UriKind.Absolute);
        client.Timeout = TimeSpan.FromSeconds(Math.Max(1, configuration.TimeoutSeconds));
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
