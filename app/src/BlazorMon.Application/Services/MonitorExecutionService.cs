using Microsoft.Extensions.Logging;
using BlazorMon.Application.Interfaces;
using BlazorMon.Domain.Enums;
using BlazorMon.Domain.Models;

namespace BlazorMon.Application.Services;

public class MonitorExecutionService(
    IMonitorRepository monitorRepo,
    IMonitorTypeRepository monitorTypeRepo,
    IEventRepository eventRepo,
    IPluginScanner pluginScanner,
    IPowerShellPluginExecutor executor,
    AlertEvaluationService alertEvaluator,
    IEmailNotificationService emailService,
    IOperatorRepository operatorRepo,
    ILogger<MonitorExecutionService> logger)
{
    public async Task ExecuteMonitorAsync(int monitorId, CancellationToken ct = default)
    {
        var monitor = await monitorRepo.GetByIdAsync(monitorId, ct);
        if (monitor is null || !monitor.IsEnabled) return;

        var monitorType = await monitorTypeRepo.GetByIdAsync(monitor.MonitorTypeId, ct);
        if (monitorType is null) return;

        var descriptor = pluginScanner.GetByKey(monitorType.TypeKey);
        if (descriptor is null)
        {
            logger.LogWarning("No plugin found for TypeKey '{TypeKey}' (Monitor: {MonitorName})", monitorType.TypeKey, monitor.Name);
            return;
        }

        Domain.Plugins.MonitorResult result;
        try
        {
            result = await executor.ExecuteAsync(descriptor.ScriptPath, monitor.MonitorXml, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Monitor '{MonitorName}' execution failed", monitor.Name);
            result = Domain.Plugins.MonitorResult.Fail($"Execution error: {ex.Message}");
        }

        var monitorEvent = new MonitorEvent
        {
            MonitorId = monitorId,
            StatusId = (int)result.Status,
            EventDt = DateTime.UtcNow,
            Message = result.Message,
            AlertSent = false
        };
        monitorEvent = await eventRepo.AddAsync(monitorEvent, ct);

        if (result.Counters.Count > 0)
        {
            var counters = result.Counters.Select(kv => new MonitorEventCounter
            {
                EventId = monitorEvent.EventId,
                MonitorId = monitorId,
                CounterName = kv.Key,
                CounterValue = kv.Value,
                EventDt = monitorEvent.EventDt
            });
            await eventRepo.AddCountersAsync(counters, ct);
        }

        var previousEvent = await eventRepo.GetLastEventAsync(monitorId, ct);
        // Note: 'previousEvent' at this point still refers to the one before the current
        // We fetched the last event, which may now be the current. Use the one before:
        MonitorEvent? priorEvent = previousEvent?.EventId == monitorEvent.EventId
            ? null
            : previousEvent;

        if (alertEvaluator.ShouldSendAlert(monitor, monitorEvent, priorEvent))
        {
            var operators = await operatorRepo.GetForMonitorAsync(monitorId, ct);
            var eligibleOperators = operators.Where(o => o.IsEnabled && IsOperatorAvailable(o)).ToList();

            if (eligibleOperators.Count > 0)
            {
                await emailService.SendAlertAsync(monitorEvent, monitor, eligibleOperators, ct);
                monitorEvent.AlertSent = true;
            }
        }

        logger.LogDebug("Monitor '{MonitorName}' => {Status}: {Message}", monitor.Name, result.Status, result.Message);
    }

    private static bool IsOperatorAvailable(Operator op)
    {
        if (op.OfflineTimeStart == "00:00" && op.OfflineTimeEnd == "00:00") return true;
        var now = DateTime.Now.TimeOfDay;
        if (!TimeSpan.TryParse(op.OfflineTimeStart, out var start) || !TimeSpan.TryParse(op.OfflineTimeEnd, out var end))
            return true;
        return start <= end ? !(now >= start && now <= end) : !(now >= start || now <= end);
    }
}
