param([hashtable]$Config = @{})

$computerName   = if ($Config.ComputerName -and $Config.ComputerName -ne 'localhost') { $Config.ComputerName } else { $env:COMPUTERNAME }
$namespace      = $Config.Namespace ?? 'root/cimv2'
$query          = $Config.Query
$propertyName   = $Config.PropertyName
$expectedValue  = $Config.ExpectedValue

if ([string]::IsNullOrWhiteSpace($query)) {
    return [PSCustomObject]@{ Status = 3; Message = "WMI Query not configured"; Counters = @{} }
}

try {
    $cimSession = New-CimSession -ComputerName $computerName -ErrorAction Stop
    $results = Get-CimInstance -CimSession $cimSession -Namespace $namespace -Query $query -ErrorAction Stop
    Remove-CimSession $cimSession

    if ($null -eq $results -or @($results).Count -eq 0) {
        return [PSCustomObject]@{
            Status = 3; Message = "WMI query returned no rows"; Counters = @{}
        }
    }

    $firstRow = @($results)[0]
    $status = 1
    $message = "WMI OK — $(@($results).Count) row(s)"
    $counterValue = 0

    if (-not [string]::IsNullOrWhiteSpace($propertyName)) {
        $propValue = $firstRow.$propertyName
        $message = "$propertyName = $propValue"

        if ($propValue -match '^\d+(\.\d+)?$') {
            $counterValue = [decimal]$propValue
        }

        if (-not [string]::IsNullOrWhiteSpace($expectedValue)) {
            if ("$propValue" -ne $expectedValue) {
                $status = 3
                $message += " (expected: $expectedValue)"
            }
        }
    }

    [PSCustomObject]@{
        Status   = $status
        Message  = $message
        Counters = @{ "Value" = $counterValue; "Row Count" = [decimal]@($results).Count }
    }
} catch {
    [PSCustomObject]@{ Status = 3; Message = "WMI error: $_"; Counters = @{} }
}
