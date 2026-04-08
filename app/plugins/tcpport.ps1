param([hashtable]$Config = @{})

$hostName  = $Config.HostName
$port      = [int]($Config.Port ?? 80)
$timeoutMs = [int]($Config.TimeoutMs ?? 3000)

if ([string]::IsNullOrWhiteSpace($hostName)) {
    return [PSCustomObject]@{ Status = 3; Message = "HostName not configured"; Counters = @{} }
}

try {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $result = Test-NetConnection -ComputerName $hostName -Port $port -WarningAction SilentlyContinue -ErrorAction Stop
    $sw.Stop()
    $elapsed = [Math]::Round($sw.Elapsed.TotalMilliseconds, 2)

    $status = if ($result.TcpTestSucceeded) { 1 } else { 3 }
    [PSCustomObject]@{
        Status   = $status
        Message  = "$hostName`:$port - $(if ($result.TcpTestSucceeded) { 'Open' } else { 'Closed/Timeout' }) ($elapsed ms)"
        Counters = @{
            "Connect ms" = $elapsed
            "Open"       = if ($result.TcpTestSucceeded) { 1 } else { 0 }
        }
    }
} catch {
    [PSCustomObject]@{ Status = 3; Message = "TCP check error: $_"; Counters = @{} }
}
