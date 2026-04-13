namespace BlazorMon.Domain.Models;

public class MonitorEventCounter
{
    public int CounterId { get; set; }
    public int EventId { get; set; }
    public int MonitorId { get; set; }
    public string CounterName { get; set; } = string.Empty;
    public decimal CounterValue { get; set; }
    public DateTime EventDt { get; set; }

    public MonitorEvent? Event { get; set; }
}
