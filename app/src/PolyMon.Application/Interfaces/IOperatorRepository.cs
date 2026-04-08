using PolyMon.Domain.Models;

namespace PolyMon.Application.Interfaces;

public interface IOperatorRepository
{
    Task<List<Operator>> GetAllAsync(CancellationToken ct = default);
    Task<Operator?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Operator>> GetForMonitorAsync(int monitorId, CancellationToken ct = default);
    Task<Operator> AddAsync(Operator op, CancellationToken ct = default);
    Task UpdateAsync(Operator op, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task SetMonitorOperatorsAsync(int monitorId, IEnumerable<int> operatorIds, CancellationToken ct = default);
}
