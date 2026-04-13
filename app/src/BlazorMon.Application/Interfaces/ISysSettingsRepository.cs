using BlazorMon.Domain.Models;

namespace BlazorMon.Application.Interfaces;

public interface ISysSettingsRepository
{
    Task<SysSettings?> GetAsync(CancellationToken ct = default);
    Task SaveAsync(SysSettings settings, CancellationToken ct = default);
}
