using Microsoft.EntityFrameworkCore;
using PolyMon.Application.Interfaces;
using PolyMon.Domain.Models;
using PolyMon.Infrastructure.Data;

namespace PolyMon.Infrastructure.Repositories;

public class DashboardRepository(PolyMonDbContext db) : IDashboardRepository
{
    public Task<List<DashboardGroup>> GetGroupsAsync(CancellationToken ct = default)
        => db.DashboardGroups
               .Include(g => g.GroupMonitors).ThenInclude(gm => gm.Monitor)
               .OrderBy(g => g.DisplayOrder)
               .ToListAsync(ct);

    public async Task<DashboardGroup> AddGroupAsync(DashboardGroup group, CancellationToken ct = default)
    {
        db.DashboardGroups.Add(group);
        await db.SaveChangesAsync(ct);
        return group;
    }

    public async Task UpdateGroupAsync(DashboardGroup group, CancellationToken ct = default)
    {
        db.DashboardGroups.Update(group);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteGroupAsync(int groupId, CancellationToken ct = default)
    {
        var g = await db.DashboardGroups.FindAsync([groupId], ct);
        if (g is not null)
        {
            db.DashboardGroups.Remove(g);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task SetGroupMonitorsAsync(int groupId, IEnumerable<int> monitorIds, CancellationToken ct = default)
    {
        var existing = db.DashboardGroupMonitors.Where(gm => gm.GroupId == groupId);
        db.DashboardGroupMonitors.RemoveRange(existing);
        var order = 0;
        db.DashboardGroupMonitors.AddRange(monitorIds.Select(mid => new DashboardGroupMonitor
        {
            GroupId = groupId, MonitorId = mid, DisplayOrder = order++
        }));
        await db.SaveChangesAsync(ct);
    }
}
