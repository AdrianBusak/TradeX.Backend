using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.EconomicCalendar.Services;

public sealed class EconomicCalendarSynchronizationService(
    ITradeXRepository repository,
    IEconomicCalendarProvider provider,
    ILogger<EconomicCalendarSynchronizationService> logger)
    : IEconomicCalendarSynchronizationService
{
    public async Task<EconomicCalendarSyncResult> SynchronizeAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Economic Calendar synchronization started.");

        var fetched = await provider.GetCurrentWeekAsync(cancellationToken).ConfigureAwait(false);
        if (!fetched.IsSuccess)
        {
            logger.LogWarning("Economic Calendar synchronization did not run: {Reason}", fetched.ErrorMessage);
            return EconomicCalendarSyncResult.Failed(fetched.ErrorMessage);
        }

        var incoming = fetched.Events
            .DistinctBy(item => new { item.Title, item.Currency, item.ScheduledAt })
            .ToList();

        if (incoming.Count == 0)
        {
            logger.LogWarning("Economic Calendar synchronization did not run because the feed had no valid events.");
            return EconomicCalendarSyncResult.Failed("Provider returned no valid events.");
        }

        var firstScheduledAt = incoming.Min(x => x.ScheduledAt);
        var lastScheduledAt = incoming.Max(x => x.ScheduledAt);
        var existing = await repository.DbContext.EconomicEvent
            .Where(x => x.ScheduledAt >= firstScheduledAt && x.ScheduledAt <= lastScheduledAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var existingByKey = existing.ToDictionary(x => new EconomicEventKey(x.Title, x.Currency, x.ScheduledAt));
        var now = DateTimeOffset.UtcNow;
        var added = 0;
        var updated = 0;

        foreach (var item in incoming)
        {
            var key = new EconomicEventKey(item.Title, item.Currency, item.ScheduledAt);
            if (existingByKey.TryGetValue(key, out var entity))
            {
                entity.Impact = item.Impact;
                entity.Forecast = item.Forecast;
                entity.Previous = item.Previous;
                entity.ModifiedAt = now;
                entity.LastSyncedAt = now;
                updated++;
                continue;
            }

            repository.DbContext.EconomicEvent.Add(new EconomicEvent
            {
                Id = Guid.NewGuid(),
                Title = item.Title,
                Currency = item.Currency,
                ScheduledAt = item.ScheduledAt,
                Impact = item.Impact,
                Forecast = item.Forecast,
                Previous = item.Previous,
                IsActive = true,
                CreatedAt = now,
                ModifiedAt = now,
                LastSyncedAt = now
            });
            added++;
        }

        await repository.DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation(
            "Economic Calendar synchronization completed. Added: {Added}; Updated: {Updated}.",
            added,
            updated);

        return new EconomicCalendarSyncResult(true, added, updated);
    }

    private sealed record EconomicEventKey(string Title, string Currency, DateTimeOffset ScheduledAt);
}
