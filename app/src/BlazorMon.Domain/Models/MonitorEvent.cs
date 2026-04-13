using BlazorMon.Domain.Enums;

namespace BlazorMon.Domain.Models;

public class MonitorEvent
{
    public int EventId { get; set; }
    public int MonitorId { get; set; }
    public int StatusId { get; set; }   // maps to MonitorStatus enum
    public DateTime EventDt { get; set; }
    public string? Message { get; set; }
    public bool AlertSent { get; set; }

    public MonitorStatus Status => (MonitorStatus)StatusId;

    public Monitor? Monitor { get; set; }
    public ICollection<MonitorEventCounter> Counters { get; set; } = [];
}
