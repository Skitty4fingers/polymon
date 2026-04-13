namespace BlazorMon.Application.Interfaces;

public interface IMonitorRepository
{
    Task<List<MonitorDef>> GetAllAsync(CancellationToken ct = default);
    Task<List<MonitorDef>> GetEnabledAsync(CancellationToken ct = default);
    Task<MonitorDef?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MonitorDef> AddAsync(MonitorDef monitor, CancellationToken ct = default);
    Task UpdateAsync(MonitorDef monitor, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
