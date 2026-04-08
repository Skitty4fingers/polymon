param([hashtable]$Config = @{})

$computerName = if ($Config.ComputerName) { $Config.ComputerName } else { $env:COMPUTERNAME }
$warnThreshold = [decimal]($Config.WarnThreshold ?? 80)
$failThreshold = [decimal]($Config.FailThreshold ?? 95)

try {
    $counter = if ($computerName -eq $env:COMPUTERNAME -or $computerName -eq 'localhost') {
        Get-Counter '\Processor(_Total)\% Processor Time' -ErrorAction Stop
    } else {
        Get-Counter "\\$computerName\Processor(_Total)\% Processor Time" -ErrorAction Stop
    }
    $cpu = [Math]::Round($counter.CounterSamples[0].CookedValue, 2)

    $status = if ($cpu -ge $failThreshold) { 3 }
              elseif ($cpu -ge $warnThreshold) { 2 }
              else { 1 }

    [PSCustomObject]@{
        Status   = $status
        Message  = "CPU: $cpu% (warn>$warnThreshold%, fail>$failThreshold%)"
        Counters = @{ "CPU %" = $cpu }
    }
} catch {
    [PSCustomObject]@{
        Status   = 3
        Message  = "Error reading CPU counter: $_"
        Counters = @{}
    }
}
