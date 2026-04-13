using PolyMon.Domain.Models;

namespace PolyMon.Application.Interfaces;

public interface IEventRepository
{
    Task<MonitorEvent?> GetLastEventAsync(int monitorId, CancellationToken ct = default);
    Task<List<MonitorEvent>> GetRecentAsync(int count, CancellationToken ct = default);
    Task<List<MonitorEvent>> GetByMonitorAsync(int monitorId, int count = 100, CancellationToken ct = default);
    Task<List<MonitorEvent>> GetActiveAlertsAsync(CancellationToken ct = default);
    Task<List<MonitorEvent>> GetHistoryAsync(DateTime from, DateTime to, int? monitorId = null, CancellationToken ct = default);
    Task<MonitorEvent> AddAsync(MonitorEvent monitorEvent, CancellationToken ct = default);
    Task<List<MonitorEventCounter>> GetCountersForEventAsync(int eventId, CancellationToken ct = default);
    Task AddCountersAsync(IEnumerable<MonitorEventCounter> counters, CancellationToken ct = default);
}
