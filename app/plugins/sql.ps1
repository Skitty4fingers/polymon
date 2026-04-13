param([hashtable]$Config = @{})

$connStr       = $Config.ConnectionString
$query         = $Config.Query
$warnThreshold = [decimal]($Config.WarnThreshold ?? 0)
$failThreshold = [decimal]($Config.FailThreshold ?? 0)
$timeoutSec    = [int]($Config.TimeoutSeconds ?? 30)

if ([string]::IsNullOrWhiteSpace($connStr) -or [string]::IsNullOrWhiteSpace($query)) {
    return [PSCustomObject]@{ Status = 3; Message = "ConnectionString or Query not configured"; Counters = @{} }
}

try {
    $conn = [System.Data.SqlClient.SqlConnection]::new($connStr)
    $conn.Open()

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $query
    $cmd.CommandTimeout = $timeoutSec

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $scalar = $cmd.ExecuteScalar()
    $sw.Stop()
    $elapsed = [Math]::Round($sw.Elapsed.TotalMilliseconds, 2)

    $conn.Close()
    $conn.Dispose()

    $value = if ($null -ne $scalar) { [decimal]$scalar } else { 0 }
    $status = if ($failThreshold -ne 0 -and $value -ge $failThreshold) { 3 }
              elseif ($warnThreshold -ne 0 -and $value -ge $warnThreshold) { 2 }
              else { 1 }

    [PSCustomObject]@{
        Status   = $status
        Message  = "SQL result: $value ($elapsed ms)"
        Counters = @{ "Value" = $value; "Query ms" = $elapsed }
    }
} catch {
    [PSCustomObject]@{ Status = 3; Message = "SQL error: $_"; Counters = @{} }
}
