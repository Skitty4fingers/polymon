using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BlazorMon.Application.Interfaces;

namespace BlazorMon.Application.BackgroundServices;

/// <summary>
/// Runs on a periodic schedule to call the SQL Server aggregation stored
/// procedures that roll raw monitor events into daily/weekly/monthly summaries
/// and purge rows exceeding retention limits.
///
/// Schedule:
///   - UpdateStatusTables / UpdateCounterTables: every <AggregationIntervalMinutes> minutes (default 60)
///   - ApplyRetentionScheme: once per day at startup + every 24 h
/// </summary>
public sealed class AggregationService(
    IServiceScopeFactory scopeFactory,
    ILogger<AggregationService> logger) : BackgroundService
{
    private static readonly TimeSpan AggregationInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan RetentionInterval   = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("AggregationService starting");

        // Stagger the first run by 5 minutes so the app is fully initialised
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        var retentionDue = DateTime.UtcNow; // run retention immediately after the delay

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAggregationAsync(stoppingToken);

                if (DateTime.UtcNow >= retentionDue)
                {
                    await RunRetentionAsync(stoppingToken);
                    retentionDue = DateTime.UtcNow.Add(RetentionInterval);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error in aggregation cycle");
            }

            await Task.Delay(AggregationInterval, stoppingToken);
        }

        logger.LogInformation("AggregationService stopping");
    }

    private async Task RunAggregationAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAggregationRepository>();

        logger.LogDebug("Running status/counter aggregation");
        await repo.UpdateStatusTablesAsync(ct);
        await repo.UpdateCounterTablesAsync(ct);
    }

    private async Task RunRetentionAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAggregationRepository>();

        logger.LogInformation("Running retention scheme");
        await repo.ApplyRetentionSchemeAsync(ct);
    }
}
