using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BlazorMon.Application.Interfaces;
using BlazorMon.Infrastructure.Data;

namespace BlazorMon.Infrastructure.Repositories;

/// <summary>
/// Calls the legacy aggregation stored procedures via raw SQL.
/// If a stored procedure does not exist in the target database the call is
/// skipped with a warning — this allows the app to start cleanly against a
/// fresh schema that does not yet have the aggregation SPs installed.
/// </summary>
public class AggregationRepository(
    PolyMonDbContext db,
    ILogger<AggregationRepository> logger) : IAggregationRepository
{
    public async Task UpdateStatusTablesAsync(CancellationToken ct = default)
        => await ExecuteIfExistsAsync("agg_UpdateStatusTables", ct);

    public async Task UpdateCounterTablesAsync(CancellationToken ct = default)
        => await ExecuteIfExistsAsync("agg_UpdateCounterTables", ct);

    public async Task ApplyRetentionSchemeAsync(CancellationToken ct = default)
        => await ExecuteIfExistsAsync("agg_ApplyRetentionScheme", ct);

    private async Task ExecuteIfExistsAsync(string spName, CancellationToken ct)
    {
        try
        {
            // Execute the SP only if it exists — graceful degradation on fresh schemas
            await db.Database.ExecuteSqlRawAsync(
                $"IF OBJECT_ID('dbo.{spName}') IS NOT NULL EXEC dbo.{spName}", ct);

            logger.LogDebug("Executed {StoredProcedure}", spName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to execute stored procedure {StoredProcedure}", spName);
        }
    }
}
