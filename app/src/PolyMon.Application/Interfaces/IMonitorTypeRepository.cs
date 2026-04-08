using PolyMon.Domain.Models;

namespace PolyMon.Application.Interfaces;

public interface IMonitorTypeRepository
{
    Task<List<MonitorType>> GetAllAsync(CancellationToken ct = default);
    Task<MonitorType?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MonitorType?> GetByKeyAsync(string typeKey, CancellationToken ct = default);
    Task<MonitorType> AddAsync(MonitorType monitorType, CancellationToken ct = default);
    Task UpdateAsync(MonitorType monitorType, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
