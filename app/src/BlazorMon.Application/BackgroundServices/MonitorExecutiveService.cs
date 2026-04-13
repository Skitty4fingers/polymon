using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BlazorMon.Application.Interfaces;
using BlazorMon.Application.Services;

namespace BlazorMon.Application.BackgroundServices;

public class MonitorExecutiveService(
    IServiceScopeFactory scopeFactory,
    ILogger<MonitorExecutiveService> logger) : BackgroundService
{
    private int _intervalSeconds = 60;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("MonitorExecutiveService starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCycleAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error in monitor execution cycle");
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }

        logger.LogInformation("MonitorExecutiveService stopping");
    }

    private async Task RunCycleAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var settingsRepo = scope.ServiceProvider.GetRequiredService<ISysSettingsRepository>();
        var monitorRepo = scope.ServiceProvider.GetRequiredService<IMonitorRepository>();
        var executionService = scope.ServiceProvider.GetRequiredService<MonitorExecutionService>();

        var settings = await settingsRepo.GetAsync(ct);
        if (settings is not null)
        {
            _intervalSeconds = settings.MainTimerInterval;
            if (!settings.IsEnabled)
            {
                logger.LogDebug("Monitoring is disabled via SysSettings");
                return;
            }
        }

        var monitors = await monitorRepo.GetEnabledAsync(ct);
        logger.LogDebug("Running {Count} monitors", monitors.Count);

        var tasks = monitors.Select(m => executionService.ExecuteMonitorAsync(m.MonitorId, ct));
        await Task.WhenAll(tasks);
    }
}
