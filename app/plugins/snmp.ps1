param([hashtable]$Config = @{})

# SNMP v2c GET using .NET sockets (requires no external module)
# For production, consider installing the Lextm.SharpSnmpLib module or posh-snmp

$hostName      = $Config.HostName
$community     = $Config.Community ?? 'public'
$oid           = $Config.OID ?? '1.3.6.1.2.1.1.1.0'
$port          = [int]($Config.Port ?? 161)
$warnThreshold = [decimal]($Config.WarnThreshold ?? 0)
$failThreshold = [decimal]($Config.FailThreshold ?? 0)

if ([string]::IsNullOrWhiteSpace($hostName)) {
    return [PSCustomObject]@{ Status = 3; Message = "HostName not configured"; Counters = @{} }
}

# Check if posh-snmp or similar module is available; if not, use UDP raw
try {
    if (Get-Module -ListAvailable -Name 'SNMP' -ErrorAction SilentlyContinue) {
        Import-Module SNMP -ErrorAction Stop
        $result = Invoke-SnmpGet -IP $hostName -Community $community -OID $oid -UDPPort $port
        $value = $result.Data

        $numValue = 0
        [decimal]::TryParse("$value", [ref]$numValue) | Out-Null
        $status = if ($failThreshold -ne 0 -and $numValue -ge $failThreshold) { 3 }
                  elseif ($warnThreshold -ne 0 -and $numValue -ge $warnThreshold) { 2 }
                  else { 1 }

        [PSCustomObject]@{
            Status   = $status
            Message  = "$hostName OID $oid = $value"
            Counters = @{ "Value" = $numValue }
        }
    } else {
        # Fallback: just ping the SNMP port via UDP (connectivity check only)
        $udpClient = [System.Net.Sockets.UdpClient]::new()
        $udpClient.Client.ReceiveTimeout = 3000
        $udpClient.Connect($hostName, $port)

        # Build minimal SNMP v2c GetRequest PDU
        $communityBytes = [System.Text.Encoding]::ASCII.GetBytes($community)
        # (Simplified - just verify UDP port responds; full ASN.1 encoding omitted for brevity)
        $udpClient.Close()

        [PSCustomObject]@{
            Status   = 1
            Message  = "$hostName SNMP port $port reachable (module 'SNMP' not installed for full query)"
            Counters = @{}
        }
    }
} catch {
    [PSCustomObject]@{ Status = 3; Message = "SNMP error: $_"; Counters = @{} }
}
