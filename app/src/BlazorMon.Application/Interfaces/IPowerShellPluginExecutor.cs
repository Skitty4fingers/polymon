using BlazorMon.Domain.Plugins;

namespace BlazorMon.Application.Interfaces;

public interface IPowerShellPluginExecutor
{
    Task<MonitorResult> ExecuteAsync(string scriptPath, string configXml, CancellationToken ct = default);
}
