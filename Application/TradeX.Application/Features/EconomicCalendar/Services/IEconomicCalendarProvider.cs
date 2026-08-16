namespace TradeX.Application.Clients.Features.EconomicCalendar.Services;

public interface IEconomicCalendarProvider
{
    Task<EconomicCalendarFetchResult> GetCurrentWeekAsync(CancellationToken cancellationToken);
}

public sealed record EconomicCalendarFetchResult(
    bool IsSuccess,
    IReadOnlyList<EconomicCalendarSourceEvent> Events,
    string? ErrorMessage = null)
{
    public static EconomicCalendarFetchResult Failed(string errorMessage)
        => new(false, [], errorMessage);

    public static EconomicCalendarFetchResult Succeeded(IReadOnlyList<EconomicCalendarSourceEvent> events)
        => new(true, events);
}

public sealed record EconomicCalendarSourceEvent(
    string Title,
    string Currency,
    DateTimeOffset ScheduledAt,
    TradeX.Domain.Enums.EconomicImpact Impact,
    string? Forecast,
    string? Previous);
