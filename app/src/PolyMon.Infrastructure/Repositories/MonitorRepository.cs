using Microsoft.EntityFrameworkCore;
using PolyMon.Application.Interfaces;
using PolyMon.Infrastructure.Data;

namespace PolyMon.Infrastructure.Repositories;

public class MonitorRepository(PolyMonDbContext db) : IMonitorRepository
{
    public Task<List<MonitorDef>> GetAllAsync(CancellationToken ct = default)
        => db.Monitors.Include(m => m.MonitorType).OrderBy(m => m.Name).ToListAsync(ct);

    public Task<List<MonitorDef>> GetEnabledAsync(CancellationToken ct = default)
        => db.Monitors.Include(m => m.MonitorType).Where(m => m.IsEnabled).ToListAsync(ct);

    public Task<MonitorDef?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.Monitors.Include(m => m.MonitorType).Include(m => m.MonitorOperators)
               .FirstOrDefaultAsync(m => m.MonitorId == id, ct);

    public async Task<MonitorDef> AddAsync(MonitorDef monitor, CancellationToken ct = default)
    {
        db.Monitors.Add(monitor);
        await db.SaveChangesAsync(ct);
        return monitor;
    }

    public async Task UpdateAsync(MonitorDef monitor, CancellationToken ct = default)
    {
        db.Monitors.Update(monitor);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var m = await db.Monitors.FindAsync([id], ct);
        if (m is not null)
        {
            db.Monitors.Remove(m);
            await db.SaveChangesAsync(ct);
        }
    }
}
