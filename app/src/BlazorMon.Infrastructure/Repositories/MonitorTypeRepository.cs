using Microsoft.EntityFrameworkCore;
using BlazorMon.Application.Interfaces;
using BlazorMon.Domain.Models;
using BlazorMon.Infrastructure.Data;

namespace BlazorMon.Infrastructure.Repositories;

public class MonitorTypeRepository(PolyMonDbContext db) : IMonitorTypeRepository
{
    public Task<List<MonitorType>> GetAllAsync(CancellationToken ct = default)
        => db.MonitorTypes.OrderBy(mt => mt.Name).ToListAsync(ct);

    public Task<MonitorType?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.MonitorTypes.FirstOrDefaultAsync(mt => mt.MonitorTypeId == id, ct);

    public Task<MonitorType?> GetByKeyAsync(string typeKey, CancellationToken ct = default)
        => db.MonitorTypes.FirstOrDefaultAsync(mt => mt.TypeKey == typeKey, ct);

    public async Task<MonitorType> AddAsync(MonitorType monitorType, CancellationToken ct = default)
    {
        db.MonitorTypes.Add(monitorType);
        await db.SaveChangesAsync(ct);
        return monitorType;
    }

    public async Task UpdateAsync(MonitorType monitorType, CancellationToken ct = default)
    {
        db.MonitorTypes.Update(monitorType);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var mt = await db.MonitorTypes.FindAsync([id], ct);
        if (mt is not null)
        {
            db.MonitorTypes.Remove(mt);
            await db.SaveChangesAsync(ct);
        }
    }
}
