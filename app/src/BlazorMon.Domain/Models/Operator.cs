namespace BlazorMon.Domain.Models;

public class Operator
{
    public int OperatorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string EmailAddress { get; set; } = string.Empty;
    public string OfflineTimeStart { get; set; } = "00:00";
    public string OfflineTimeEnd { get; set; } = "00:00";
    public bool IncludeMessageBody { get; set; } = true;

    // Queued (immediate) notifications: 0=None, 1=Fail, 2=Warn+Fail
    public byte QueuedNotify { get; set; } = 1;

    // Summary digest notifications
    public bool SummaryNotify { get; set; }
    public bool SummaryNotifyOk { get; set; }
    public bool SummaryNotifyWarn { get; set; } = true;
    public bool SummaryNotifyFail { get; set; } = true;
    public string SummaryNotifyTime { get; set; } = "08:00";
    public DateTime SummaryNextNotifyDt { get; set; } = DateTime.UtcNow;

    public ICollection<MonitorOperator> MonitorOperators { get; set; } = [];
}
