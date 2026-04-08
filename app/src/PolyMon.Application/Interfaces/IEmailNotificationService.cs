using PolyMon.Domain.Models;

namespace PolyMon.Application.Interfaces;

public interface IEmailNotificationService
{
    Task SendAlertAsync(MonitorEvent monitorEvent, MonitorDef monitor, IEnumerable<Operator> operators, CancellationToken ct = default);
    Task SendSummaryAsync(Operator op, IEnumerable<MonitorEvent> recentEvents, CancellationToken ct = default);
}
