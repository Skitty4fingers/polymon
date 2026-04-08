using PolyMon.Domain.Models;

namespace PolyMon.Application.Interfaces;

public interface ISysSettingsRepository
{
    Task<SysSettings?> GetAsync(CancellationToken ct = default);
    Task SaveAsync(SysSettings settings, CancellationToken ct = default);
}
