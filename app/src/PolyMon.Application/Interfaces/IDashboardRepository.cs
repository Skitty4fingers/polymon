using PolyMon.Domain.Models;

namespace PolyMon.Application.Interfaces;

public interface IDashboardRepository
{
    Task<List<DashboardGroup>> GetGroupsAsync(CancellationToken ct = default);
    Task<DashboardGroup> AddGroupAsync(DashboardGroup group, CancellationToken ct = default);
    Task UpdateGroupAsync(DashboardGroup group, CancellationToken ct = default);
    Task DeleteGroupAsync(int groupId, CancellationToken ct = default);
    Task SetGroupMonitorsAsync(int groupId, IEnumerable<int> monitorIds, CancellationToken ct = default);
}
