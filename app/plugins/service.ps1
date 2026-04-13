param([hashtable]$Config = @{})

$serviceName = $Config.ServiceName
$computerName = if ($Config.ComputerName -and $Config.ComputerName -ne 'localhost') { $Config.ComputerName } else { $env:COMPUTERNAME }

if ([string]::IsNullOrWhiteSpace($serviceName)) {
    return [PSCustomObject]@{ Status = 3; Message = "ServiceName not configured"; Counters = @{} }
}

try {
    $svc = Get-Service -Name $serviceName -ComputerName $computerName -ErrorAction Stop
    $running = $svc.Status -eq 'Running'

    $status = if ($running) { 1 } else { 3 }
    [PSCustomObject]@{
        Status   = $status
        Message  = "'$($svc.DisplayName)' is $($svc.Status)"
        Counters = @{ "Running" = if ($running) { 1 } else { 0 } }
    }
} catch {
    [PSCustomObject]@{ Status = 3; Message = "Service check error: $_"; Counters = @{} }
}
