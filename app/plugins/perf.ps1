param([hashtable]$Config = @{})

$counterPath = $Config.CounterPath
$computerName = $Config.ComputerName
$warnThreshold = [decimal]($Config.WarnThreshold ?? 0)
$failThreshold = [decimal]($Config.FailThreshold ?? 0)
$direction = $Config.ThresholdDirection ?? 'above'

if ([string]::IsNullOrWhiteSpace($counterPath)) {
    return [PSCustomObject]@{ Status = 3; Message = "CounterPath not configured"; Counters = @{} }
}

try {
    $fullPath = if ($computerName -and $computerName -ne 'localhost') {
        "\\$computerName$counterPath"
    } else {
        $counterPath
    }

    $result = Get-Counter $fullPath -ErrorAction Stop
    $value = [Math]::Round($result.CounterSamples[0].CookedValue, 4)

    $status = 1
    if ($failThreshold -ne 0) {
        $status = if ($direction -eq 'below') {
            if ($value -le $failThreshold) { 3 } elseif ($warnThreshold -ne 0 -and $value -le $warnThreshold) { 2 } else { 1 }
        } else {
            if ($value -ge $failThreshold) { 3 } elseif ($warnThreshold -ne 0 -and $value -ge $warnThreshold) { 2 } else { 1 }
        }
    }

    [PSCustomObject]@{
        Status   = $status
        Message  = "$counterPath = $value"
        Counters = @{ "Value" = $value }
    }
} catch {
    [PSCustomObject]@{ Status = 3; Message = "Perf counter error: $_"; Counters = @{} }
}
