namespace PolyMon.Application.Interfaces;

/// <summary>
/// Executes the time-series aggregation stored procedures that roll up raw
/// MonitorEvent / MonitorEventCounter data into daily, weekly, and monthly
/// summary tables (AggStatus_*, AggCounter_*).
/// </summary>
public interface IAggregationRepository
{
    /// <summary>Rolls up raw status events into AggStatus_Daily/Weekly/Monthly.</summary>
    Task UpdateStatusTablesAsync(CancellationToken ct = default);

    /// <summary>Rolls up raw counter events into AggCounter_Daily/Weekly/Monthly.</summary>
    Task UpdateCounterTablesAsync(CancellationToken ct = default);

    /// <summary>Purges rows older than the per-monitor retention limits.</summary>
    Task ApplyRetentionSchemeAsync(CancellationToken ct = default);
}
