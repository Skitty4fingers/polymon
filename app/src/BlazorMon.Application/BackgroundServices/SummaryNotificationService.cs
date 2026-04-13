using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BlazorMon.Application.Interfaces;

namespace BlazorMon.Application.BackgroundServices;

public class SummaryNotificationService(
    IServiceScopeFactory scopeFactory,
    ILogger<SummaryNotificationService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessSummariesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error processing summary notifications");
            }

            // Check every minute
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessSummariesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var operatorRepo = scope.ServiceProvider.GetRequiredService<IOperatorRepository>();
        var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();

        var operators = await operatorRepo.GetAllAsync(ct);
        var now = DateTime.UtcNow;

        foreach (var op in operators.Where(o => o.IsEnabled && o.SummaryNotify && o.SummaryNextNotifyDt <= now))
        {
            var recentEvents = await eventRepo.GetHistoryAsync(now.AddHours(-24), now, ct: ct);
            await emailService.SendSummaryAsync(op, recentEvents, ct);

            // Schedule next summary notification
            if (TimeSpan.TryParse(op.SummaryNotifyTime, out var time))
                op.SummaryNextNotifyDt = DateTime.UtcNow.Date.Add(time).AddDays(1);
            else
                op.SummaryNextNotifyDt = DateTime.UtcNow.AddDays(1);

            await operatorRepo.UpdateAsync(op, ct);
            logger.LogInformation("Summary notification sent to {OperatorName}", op.Name);
        }
    }
}
