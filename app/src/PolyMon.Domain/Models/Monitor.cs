namespace PolyMon.Domain.Models;

public class Monitor
{
    public int MonitorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int MonitorTypeId { get; set; }
    public string MonitorXml { get; set; } = string.Empty;   // plugin config as XML
    public int TriggerMod { get; set; } = 1;

    // Offline time windows (suppress alerts)
    public string OfflineTime1Start { get; set; } = "00:00";
    public string OfflineTime1End { get; set; } = "00:00";
    public string OfflineTime2Start { get; set; } = "00:00";
    public string OfflineTime2End { get; set; } = "00:00";

    // Alert message templates
    public string MessageSubjectTemplate { get; set; } = string.Empty;
    public string? MessageBodyTemplate { get; set; }

    // Alert rules
    public int AlertAfterEveryNEvent { get; set; } = 1;
    public bool AlertAfterEveryNewFailure { get; set; } = true;
    public int AlertAfterEveryNFailures { get; set; } = 1;
    public bool AlertAfterEveryFailToOk { get; set; } = true;
    public bool AlertAfterEveryNewWarning { get; set; } = false;
    public int AlertAfterEveryNWarnings { get; set; } = 1;
    public bool AlertAfterEveryWarnToOk { get; set; } = false;

    // Retention policies (months)
    public int MaxMonthsRaw { get; set; } = 24;
    public int MaxMonthsDaily { get; set; } = 36;
    public int MaxMonthsWeekly { get; set; } = 60;
    public int MaxMonthsMonthly { get; set; } = 60;

    // After-event script
    public bool AfterEventIsEnabled { get; set; }
    public int AfterEventScriptEngineId { get; set; }
    public string? AfterEventScript { get; set; }

    public MonitorType? MonitorType { get; set; }
    public ICollection<MonitorEvent> Events { get; set; } = [];
    public ICollection<MonitorOperator> MonitorOperators { get; set; } = [];
}
