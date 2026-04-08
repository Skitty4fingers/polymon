param([hashtable]$Config = @{})

$host      = $Config.HostName
$count     = [int]($Config.PingCount ?? 4)
$timeoutMs = [int]($Config.TimeoutMs ?? 1000)
$warnRtt   = [decimal]($Config.WarnRttMs ?? 100)
$failLoss  = [decimal]($Config.FailPacketLossPct ?? 50)

if ([string]::IsNullOrWhiteSpace($host)) {
    return [PSCustomObject]@{ Status = 3; Message = "HostName not configured"; Counters = @{} }
}

try {
    $results = 1..$count | ForEach-Object {
        Test-Connection -ComputerName $host -Count 1 -TimeoutSeconds ([int]($timeoutMs/1000 + 1)) -ErrorAction SilentlyContinue
    }

    $responded = @($results | Where-Object { $_ -ne $null })
    $lostCount = $count - $responded.Count
    $lossPct   = [Math]::Round($lostCount / $count * 100, 2)
    $avgRtt    = if ($responded.Count -gt 0) {
        [Math]::Round(($responded | Measure-Object -Property ResponseTime -Average).Average, 2)
    } else { 0 }

    $status = if ($lossPct -ge $failLoss -or $responded.Count -eq 0) { 3 }
              elseif ($avgRtt -ge $warnRtt) { 2 }
              else { 1 }

    [PSCustomObject]@{
        Status   = $status
        Message  = "$host - RTT: $avgRtt ms, Loss: $lossPct%"
        Counters = @{ "RTT ms" = $avgRtt; "Loss %" = $lossPct }
    }
} catch {
    [PSCustomObject]@{ Status = 3; Message = "Ping error: $_"; Counters = @{} }
}
