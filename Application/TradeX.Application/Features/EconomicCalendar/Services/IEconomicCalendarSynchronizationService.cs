namespace TradeX.Application.Clients.Features.EconomicCalendar.Services;

public interface IEconomicCalendarSynchronizationService
{
    Task<EconomicCalendarSyncResult> SynchronizeAsync(CancellationToken cancellationToken);
}

public sealed record EconomicCalendarSyncResult(
    bool IsSuccess,
    int Added,
    int Updated,
    string? ErrorMessage = null)
{
    public static EconomicCalendarSyncResult Failed(string? errorMessage)
        => new(false, 0, 0, errorMessage);
}
