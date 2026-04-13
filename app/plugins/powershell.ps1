param([hashtable]$Config = @{})

$script        = $Config.Script
$timeoutSec    = [int]($Config.TimeoutSeconds ?? 30)

if ([string]::IsNullOrWhiteSpace($script)) {
    return [PSCustomObject]@{ Status = 3; Message = "Script not configured"; Counters = @{} }
}

try {
    $job = Start-Job -ScriptBlock ([scriptblock]::Create($script))
    $completed = Wait-Job $job -Timeout $timeoutSec

    if ($null -eq $completed) {
        Stop-Job $job
        Remove-Job $job -Force
        return [PSCustomObject]@{
            Status = 3; Message = "Script timed out after $timeoutSec seconds"; Counters = @{}
        }
    }

    $result = Receive-Job $job
    Remove-Job $job

    if ($null -eq $result) {
        return [PSCustomObject]@{ Status = 3; Message = "Script returned no output"; Counters = @{} }
    }

    # Validate and normalize output
    $status = if ($result.Status -in 1,2,3) { [int]$result.Status } else { 1 }
    $message = if ($result.Message) { "$($result.Message)" } else { "Script completed" }
    $counters = if ($result.Counters -is [hashtable]) { $result.Counters } else { @{} }

    [PSCustomObject]@{
        Status   = $status
        Message  = $message
        Counters = $counters
    }
} catch {
    [PSCustomObject]@{ Status = 3; Message = "PowerShell monitor error: $_"; Counters = @{} }
}
