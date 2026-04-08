param([hashtable]$Config = @{})

$computerName = if ($Config.ComputerName -and $Config.ComputerName -ne 'localhost') { $Config.ComputerName } else { $env:COMPUTERNAME }
$drive = ($Config.DriveLetter ?? 'C').TrimEnd(':') + ':'
$warnFree = [decimal]($Config.WarnFreePercent ?? 20)
$failFree  = [decimal]($Config.FailFreePercent ?? 10)

try {
    $disk = Get-CimInstance -ClassName Win32_LogicalDisk -ComputerName $computerName `
            -Filter "DeviceID='$drive'" -ErrorAction Stop

    if ($null -eq $disk) {
        return [PSCustomObject]@{
            Status = 3; Message = "Drive $drive not found on $computerName"; Counters = @{}
        }
    }

    $freeGB   = [Math]::Round($disk.FreeSpace / 1GB, 2)
    $totalGB  = [Math]::Round($disk.Size / 1GB, 2)
    $freePct  = [Math]::Round($disk.FreeSpace / $disk.Size * 100, 2)

    $status = if ($freePct -le $failFree) { 3 }
              elseif ($freePct -le $warnFree) { 2 }
              else { 1 }

    [PSCustomObject]@{
        Status   = $status
        Message  = "$drive free: $freeGB GB ($freePct%) of $totalGB GB"
        Counters = @{
            "Free GB"  = $freeGB
            "Free %"   = $freePct
            "Total GB" = $totalGB
        }
    }
} catch {
    [PSCustomObject]@{
        Status = 3; Message = "Error checking disk: $_"; Counters = @{}
    }
}
