using TradeX.Application.Clients.Features.EconomicCalendar.Services;
using TradeX.Infrastructure.EconomicCalendar.Configuration;

namespace TradeX.API.Services;

public sealed class EconomicCalendarSyncWorker(
    IServiceScopeFactory scopeFactory,
    EconomicCalendarConfiguration configuration,
    ILogger<EconomicCalendarSyncWorker> logger)
    : BackgroundService
{
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SynchronizeAsync(stoppingToken).ConfigureAwait(false);

        using var timer = new PeriodicTimer(
            TimeSpan.FromMinutes(Math.Max(1, configuration.SyncIntervalMinutes)));
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            await SynchronizeAsync(stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        if (!await _syncLock.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            logger.LogWarning("Skipped Economic Calendar synchronization because one is already running.");
            return;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            var synchronizer = scope.ServiceProvider.GetRequiredService<IEconomicCalendarSynchronizationService>();
            var result = await synchronizer.SynchronizeAsync(cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
                logger.LogWarning("Economic Calendar synchronization failed: {Reason}", result.ErrorMessage);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Application shutdown requested.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Economic Calendar synchronization failed unexpectedly.");
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public override void Dispose()
    {
        _syncLock.Dispose();
        base.Dispose();
    }
}
