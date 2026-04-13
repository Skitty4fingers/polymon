using Microsoft.EntityFrameworkCore;
using PolyMon.Application.Interfaces;
using PolyMon.Domain.Models;
using PolyMon.Infrastructure.Data;

namespace PolyMon.Infrastructure.Repositories;

public class OperatorRepository(PolyMonDbContext db) : IOperatorRepository
{
    public Task<List<Operator>> GetAllAsync(CancellationToken ct = default)
        => db.Operators.OrderBy(o => o.Name).ToListAsync(ct);

    public Task<Operator?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.Operators.FirstOrDefaultAsync(o => o.OperatorId == id, ct);

    public Task<List<Operator>> GetForMonitorAsync(int monitorId, CancellationToken ct = default)
        => db.MonitorOperators
               .Where(mo => mo.MonitorId == monitorId)
               .Select(mo => mo.Operator!)
               .ToListAsync(ct);

    public async Task<Operator> AddAsync(Operator op, CancellationToken ct = default)
    {
        db.Operators.Add(op);
        await db.SaveChangesAsync(ct);
        return op;
    }

    public async Task UpdateAsync(Operator op, CancellationToken ct = default)
    {
        db.Operators.Update(op);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var op = await db.Operators.FindAsync([id], ct);
        if (op is not null)
        {
            db.Operators.Remove(op);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task SetMonitorOperatorsAsync(int monitorId, IEnumerable<int> operatorIds, CancellationToken ct = default)
    {
        var existing = db.MonitorOperators.Where(mo => mo.MonitorId == monitorId);
        db.MonitorOperators.RemoveRange(existing);
        db.MonitorOperators.AddRange(operatorIds.Select(oid => new MonitorOperator { MonitorId = monitorId, OperatorId = oid }));
        await db.SaveChangesAsync(ct);
    }
}
