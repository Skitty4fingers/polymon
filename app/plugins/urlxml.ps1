param([hashtable]$Config = @{})

# Security: XXE disabled by using XmlReaderSettings with DtdProcessing=Prohibit
# PowerShell's [xml] cast can be vulnerable; we use explicit XmlReader settings

$url        = $Config.Url
$xpathExpr  = $Config.XPath
$warnValue  = [decimal]($Config.WarnValue ?? 0)
$failValue  = [decimal]($Config.FailValue ?? 0)
$timeoutSec = [int]([decimal]($Config.TimeoutMs ?? 10000) / 1000)

if ([string]::IsNullOrWhiteSpace($url)) {
    return [PSCustomObject]@{ Status = 3; Message = "URL not configured"; Counters = @{} }
}

try {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $response = Invoke-WebRequest -Uri $url -TimeoutSec $timeoutSec -UseBasicParsing -ErrorAction Stop
    $sw.Stop()
    $elapsed = [Math]::Round($sw.Elapsed.TotalMilliseconds, 2)

    # Secure XML parsing — disable DTD/external entity processing
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null

    $strReader = [System.IO.StringReader]::new($response.Content)
    $xmlReader = [System.Xml.XmlReader]::Create($strReader, $settings)
    $xmlDoc = [System.Xml.XmlDocument]::new()
    $xmlDoc.XmlResolver = $null
    $xmlDoc.Load($xmlReader)
    $xmlReader.Dispose()

    $xpathValue = $null
    $status = 1

    if (-not [string]::IsNullOrWhiteSpace($xpathExpr)) {
        $node = $xmlDoc.SelectSingleNode($xpathExpr)
        if ($null -ne $node) {
            $xpathValue = [decimal]$node.InnerText
            $status = if ($failValue -gt 0 -and $xpathValue -ge $failValue) { 3 }
                      elseif ($warnValue -gt 0 -and $xpathValue -ge $warnValue) { 2 }
                      else { 1 }
        }
    }

    $msg = "$url - $elapsed ms"
    if ($null -ne $xpathValue) { $msg += " | XPath value: $xpathValue" }

    [PSCustomObject]@{
        Status   = $status
        Message  = $msg
        Counters = @{
            "Response ms" = $elapsed
            "XPath Value" = if ($null -ne $xpathValue) { $xpathValue } else { 0 }
        }
    }
} catch [System.Xml.XmlException] {
    [PSCustomObject]@{ Status = 3; Message = "XML parse error (possible XXE attempt rejected): $_"; Counters = @{} }
} catch {
    [PSCustomObject]@{ Status = 3; Message = "URL XML check error: $_"; Counters = @{} }
}
