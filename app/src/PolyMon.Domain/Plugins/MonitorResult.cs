using PolyMon.Domain.Enums;

namespace PolyMon.Domain.Plugins;

public record MonitorResult(
    MonitorStatus Status,
    string Message,
    IReadOnlyDictionary<string, decimal> Counters
)
{
    public static MonitorResult Ok(string message, Dictionary<string, decimal>? counters = null)
        => new(MonitorStatus.Ok, message, counters ?? new Dictionary<string, decimal>());

    public static MonitorResult Warning(string message, Dictionary<string, decimal>? counters = null)
        => new(MonitorStatus.Warning, message, counters ?? new Dictionary<string, decimal>());

    public static MonitorResult Fail(string message, Dictionary<string, decimal>? counters = null)
        => new(MonitorStatus.Fail, message, counters ?? new Dictionary<string, decimal>());
}
