using Microsoft.EntityFrameworkCore;
using BlazorMon.Application.Interfaces;
using BlazorMon.Domain.Enums;
using BlazorMon.Domain.Models;
using BlazorMon.Infrastructure.Data;

namespace BlazorMon.Infrastructure.Repositories;

public class EventRepository(PolyMonDbContext db) : IEventRepository
{
    public Task<MonitorEvent?> GetLastEventAsync(int monitorId, CancellationToken ct = default)
        => db.MonitorEvents
               .Where(e => e.MonitorId == monitorId)
               .OrderByDescending(e => e.EventDt)
               .FirstOrDefaultAsync(ct);

    public Task<List<MonitorEvent>> GetRecentAsync(int count, CancellationToken ct = default)
        => db.MonitorEvents.Include(e => e.Monitor)
               .OrderByDescending(e => e.EventDt)
               .Take(count)
               .ToListAsync(ct);

    public Task<List<MonitorEvent>> GetByMonitorAsync(int monitorId, int count = 100, CancellationToken ct = default)
        => db.MonitorEvents
               .Where(e => e.MonitorId == monitorId)
               .OrderByDescending(e => e.EventDt)
               .Take(count)
               .ToListAsync(ct);

    public Task<List<MonitorEvent>> GetActiveAlertsAsync(CancellationToken ct = default)
        => db.MonitorEvents.Include(e => e.Monitor)
               .Where(e => e.StatusId != (int)MonitorStatus.Ok)
               .GroupBy(e => e.MonitorId)
               .Select(g => g.OrderByDescending(e => e.EventDt).First())
               .ToListAsync(ct);

    public Task<List<MonitorEvent>> GetHistoryAsync(DateTime from, DateTime to, int? monitorId = null, CancellationToken ct = default)
    {
        var q = db.MonitorEvents.Include(e => e.Monitor)
                  .Where(e => e.EventDt >= from && e.EventDt <= to);
        if (monitorId.HasValue) q = q.Where(e => e.MonitorId == monitorId.Value);
        return q.OrderByDescending(e => e.EventDt).ToListAsync(ct);
    }

    public async Task<MonitorEvent> AddAsync(MonitorEvent monitorEvent, CancellationToken ct = default)
    {
        db.MonitorEvents.Add(monitorEvent);
        await db.SaveChangesAsync(ct);
        return monitorEvent;
    }

    public Task<List<MonitorEventCounter>> GetCountersForEventAsync(int eventId, CancellationToken ct = default)
        => db.MonitorEventCounters.Where(c => c.EventId == eventId).ToListAsync(ct);

    public async Task AddCountersAsync(IEnumerable<MonitorEventCounter> counters, CancellationToken ct = default)
    {
        db.MonitorEventCounters.AddRange(counters);
        await db.SaveChangesAsync(ct);
    }
}
