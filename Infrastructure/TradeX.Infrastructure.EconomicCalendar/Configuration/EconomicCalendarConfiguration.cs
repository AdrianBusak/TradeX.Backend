namespace TradeX.Infrastructure.EconomicCalendar.Configuration;

public sealed class EconomicCalendarConfiguration
{
    public string Url { get; set; } = "https://nfs.faireconomy.media/ff_calendar_thisweek.json";
    public int SyncIntervalMinutes { get; set; } = 15;
    public int TimeoutSeconds { get; set; } = 10;
}
