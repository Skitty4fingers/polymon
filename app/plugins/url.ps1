param([hashtable]$Config = @{})

$url            = $Config.Url
$expectedCode   = [int]($Config.ExpectedStatusCode ?? 200)
$containsText   = $Config.ContainsText
$warnMs         = [decimal]($Config.WarnResponseMs ?? 2000)
$failMs         = [decimal]($Config.FailResponseMs ?? 5000)
$timeoutSec     = [int]([decimal]($Config.TimeoutMs ?? 10000) / 1000)

if ([string]::IsNullOrWhiteSpace($url)) {
    return [PSCustomObject]@{ Status = 3; Message = "URL not configured"; Counters = @{} }
}

try {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $response = Invoke-WebRequest -Uri $url -TimeoutSec $timeoutSec -UseBasicParsing -ErrorAction Stop
    $sw.Stop()
    $elapsed = [Math]::Round($sw.Elapsed.TotalMilliseconds, 2)

    $statusCode = [int]$response.StatusCode
    $codeOk = $statusCode -eq $expectedCode
    $textOk = if ([string]::IsNullOrWhiteSpace($containsText)) { $true }
              else { $response.Content -match [regex]::Escape($containsText) }

    $status = if (-not $codeOk -or -not $textOk -or $elapsed -ge $failMs) { 3 }
              elseif ($elapsed -ge $warnMs) { 2 }
              else { 1 }

    $msg = "$url - HTTP $statusCode - $elapsed ms"
    if (-not $textOk) { $msg += " (expected text not found)" }

    [PSCustomObject]@{
        Status   = $status
        Message  = $msg
        Counters = @{ "Response ms" = $elapsed; "HTTP Status" = $statusCode }
    }
} catch {
    [PSCustomObject]@{
        Status   = 3
        Message  = "URL check error: $_"
        Counters = @{}
    }
}
