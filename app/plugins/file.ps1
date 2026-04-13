param([hashtable]$Config = @{})

$path      = $Config.Path
$monType   = $Config.MonitorType ?? 'age'
$warnThr   = [decimal]($Config.WarnThreshold ?? 60)
$failThr   = [decimal]($Config.FailThreshold ?? 120)
$direction = $Config.ThresholdDirection ?? 'above'

if ([string]::IsNullOrWhiteSpace($path)) {
    return [PSCustomObject]@{ Status = 3; Message = "Path not configured"; Counters = @{} }
}

try {
    $files = @(Get-Item $path -ErrorAction Stop)

    if ($monType -eq 'count') {
        $count = [decimal]$files.Count
        $status = Invoke-ThresholdCheck $count $warnThr $failThr $direction
        [PSCustomObject]@{
            Status   = $status
            Message  = "$path - $count file(s)"
            Counters = @{ "File Count" = $count }
        }
    } else {
        # Age monitor — find newest file
        $newest = $files | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($null -eq $newest) {
            return [PSCustomObject]@{ Status = 3; Message = "No files found at $path"; Counters = @{} }
        }
        $ageMinutes = [Math]::Round((New-TimeSpan -Start $newest.LastWriteTime -End (Get-Date)).TotalMinutes, 2)
        $status = Invoke-ThresholdCheck $ageMinutes $warnThr $failThr $direction
        [PSCustomObject]@{
            Status   = $status
            Message  = "$($newest.Name) - age: $ageMinutes min"
            Counters = @{ "Age (min)" = $ageMinutes; "File Count" = [decimal]$files.Count }
        }
    }
} catch {
    [PSCustomObject]@{ Status = 3; Message = "File monitor error: $_"; Counters = @{} }
}

function Invoke-ThresholdCheck([decimal]$value, [decimal]$warn, [decimal]$fail, [string]$dir) {
    if ($dir -eq 'below') {
        if ($value -le $fail) { return 3 }
        if ($value -le $warn) { return 2 }
        return 1
    } else {
        if ($value -ge $fail) { return 3 }
        if ($value -ge $warn) { return 2 }
        return 1
    }
}
