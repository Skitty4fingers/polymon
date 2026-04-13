# BlazorMon

Open-source infrastructure monitoring platform — modernized from the original .NET Framework 2.0 / VB.NET codebase to **.NET 10** with a **Blazor Server** frontend.

---

## What is PolyMon?

PolyMon monitors infrastructure endpoints (servers, services, URLs, databases, network devices) on a configurable schedule, logs results to SQL Server, and sends email alerts when status changes. A real-time web dashboard shows the current state of all monitors.

---

## Architecture

| Layer | Technology |
|---|---|
| Frontend | Blazor Server (.NET 10) + MudBlazor 9 |
| Monitoring engine | `BackgroundService` (replaces the Windows Service) |
| Data access | EF Core 10 + SQL Server |
| Plugins | PowerShell scripts (`.ps1` + `.plugin.json`) |
| Email | MailKit 4 |
| Authentication | ASP.NET Core Identity |
| Logging | Serilog |

### Plugin system

Each monitor type is a pair of files in `app/plugins/`:

```
plugins/
├── cpu.ps1              # monitor logic
├── cpu.plugin.json      # parameter schema + metadata
├── disk.ps1 / disk.plugin.json
└── ...
```

Plugins are scanned at startup. Adding, removing, or modifying a plugin **does not require recompiling the application** — place the two files in `plugins/` and restart.

**Available plugins:** cpu · disk · ping · service · tcpport · url · urlxml · perf · wmi · sql · snmp · file · powershell

### Solution structure

```
app/
├── db/init.sql                  ← run once against an empty SQL Server database
├── plugins/                     ← PowerShell monitor plugins
└── src/
    ├── BlazorMon.Domain/          ← models, enums, plugin contracts
    ├── BlazorMon.Application/     ← business logic, background services
    ├── BlazorMon.Infrastructure/  ← EF Core, Identity, repositories, email
    └── BlazorMon.Web/             ← Blazor Server UI entry point
```

---

## Getting started

### Prerequisites

- .NET 10 SDK
- SQL Server 2019+ (or Azure SQL)
- Windows (required for WMI, Performance Counter, and Windows Service plugins)

### 1 — Create the database

Run `app/db/init.sql` against an empty SQL Server database:

```sql
-- In SSMS or sqlcmd:
USE PolyMon;
GO
-- paste / execute init.sql
```

### 2 — Configure the connection string

Edit `app/src/BlazorMon.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost;Initial Catalog=BlazorMon;Integrated Security=SSPI;TrustServerCertificate=true"
  }
}
```

For secrets management in production, use `dotnet user-secrets` or environment variables:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."
```

### 3 — Run

```bash
cd app
dotnet run --project src/BlazorMon.Web
```

The app starts at `https://localhost:5001`. Log in with the default credentials created on first startup:

| Email | Password |
|---|---|
| `admin@blazormon.local` | `PolyMon1!` |

> **Change this password immediately** via Admin → Users after first login.

---

## Deploying as a Windows Service

The web host is pre-configured with `UseWindowsService()`. To install:

```powershell
sc.exe create "BlazorMon" binpath="C:\polymon\BlazorMon.Web.exe" start=auto
sc.exe start "BlazorMon"
```

---

## Background services

| Service | Purpose |
|---|---|
| `MonitorExecutiveService` | Main polling loop — runs all enabled monitors on the configured interval |
| `SummaryNotificationService` | Sends daily digest emails to operators with `SummaryNotify = true` |
| `AggregationService` | Calls `agg_UpdateStatusTables`, `agg_UpdateCounterTables` every hour and `agg_ApplyRetentionScheme` every 24 h |

The aggregation stored procedures are optional — they are skipped gracefully if not present in the database (fresh installs). Existing deployments upgrading from PolyMon 1.x that already have the `agg_*` procedures installed will benefit automatically.

---

## Writing a custom plugin

Create two files in `app/plugins/`:

**`myplugin.plugin.json`**

```json
{
  "TypeKey": "MyPlugin",
  "DisplayName": "My Custom Monitor",
  "Description": "Does something useful",
  "Parameters": [
    { "Name": "Target",        "Type": "string",  "Default": "localhost", "Required": true },
    { "Name": "WarnThreshold", "Type": "decimal", "Default": 80 },
    { "Name": "FailThreshold", "Type": "decimal", "Default": 95 }
  ]
}
```

**`myplugin.ps1`**

```powershell
param([hashtable]$Config = @{})

$target = $Config.Target ?? "localhost"
$warn   = [decimal]($Config.WarnThreshold ?? 80)
$fail   = [decimal]($Config.FailThreshold ?? 95)

# ... your monitoring logic ...
$value = 42.0

$status = if ($value -ge $fail) { 3 } elseif ($value -ge $warn) { 2 } else { 1 }

[PSCustomObject]@{
    Status   = $status          # 1=OK  2=Warning  3=Fail
    Message  = "Value: $value"
    Counters = @{ "MyMetric" = $value }
}
```

Restart the app and the plugin appears in Admin → Monitor Types, ready to use.

---

## Admin pages

| Page | Path |
|---|---|
| Dashboard | `/` |
| Current status | `/status/current` |
| Active alerts | `/status/alerts` |
| Event log | `/status/events` |
| Reports | `/reports` |
| Monitors | `/admin/monitors` |
| Monitor types (plugins) | `/admin/monitor-types` |
| Operators | `/admin/operators` |
| Settings (SMTP, timer) | `/admin/settings` |

---

## Security

- **Authentication**: ASP.NET Core Identity with cookie sessions. All pages require login.
- **XXE**: The `urlxml` plugin parses XML with `DtdProcessing = DtdProcessing.Prohibit` — external entity injection is blocked.
- **SMTP credentials**: Stored in `SysSettings` (database). Use environment variables or `dotnet user-secrets` to avoid hardcoding connection strings.
- **Password policy**: Minimum 8 characters. Account lockout enabled after failed attempts.

---

## Development

```bash
# Build
dotnet build app/

# Test (32 unit tests)
dotnet test app/

# Run in development (hot reload)
dotnet watch --project app/src/BlazorMon.Web
```

---

## Original project

PolyMon was originally created by Fred Baptiste and published on CodePlex.  
The legacy archive: https://archive.codeplex.com/?p=polymon
