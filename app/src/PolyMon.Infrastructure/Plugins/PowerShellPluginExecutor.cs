using System.Collections;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using PolyMon.Application.Interfaces;
using PolyMon.Domain.Enums;
using PolyMon.Domain.Plugins;

namespace PolyMon.Infrastructure.Plugins;

public class PowerShellPluginExecutor(ILogger<PowerShellPluginExecutor> logger) : IPowerShellPluginExecutor
{
    public async Task<MonitorResult> ExecuteAsync(string scriptPath, string configXml, CancellationToken ct = default)
    {
        var config = ParseConfigXml(configXml);

        return await Task.Run(() =>
        {
            using var ps = PowerShell.Create();
            ps.AddCommand(scriptPath);
            ps.AddParameter("Config", config);

            try
            {
                var results = ps.Invoke();

                if (ps.HadErrors)
                {
                    var errorMsg = string.Join("; ", ps.Streams.Error.Select(e => e.ToString()));
                    logger.LogWarning("PowerShell script '{Script}' reported errors: {Errors}", scriptPath, errorMsg);
                    return MonitorResult.Fail($"Script error: {errorMsg}");
                }

                if (results.Count == 0 || results[0] is null)
                    return MonitorResult.Fail("Script returned no output");

                return ParsePsResult(results[0]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to execute plugin script '{Script}'", scriptPath);
                return MonitorResult.Fail($"Execution exception: {ex.Message}");
            }
        }, ct);
    }

    private static Hashtable ParseConfigXml(string configXml)
    {
        var ht = new Hashtable(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(configXml)) return ht;

        try
        {
            var doc = XDocument.Parse(configXml);
            foreach (var el in doc.Root?.Elements() ?? Enumerable.Empty<XElement>())
                ht[el.Name.LocalName] = el.Value;
        }
        catch
        {
            // Return empty config if XML is invalid
        }

        return ht;
    }

    private static MonitorResult ParsePsResult(PSObject psObject)
    {
        var statusVal = psObject.Properties["Status"]?.Value;
        var message = psObject.Properties["Message"]?.Value?.ToString() ?? string.Empty;
        var countersObj = psObject.Properties["Counters"]?.Value;

        var status = statusVal switch
        {
            int i => (MonitorStatus)i,
            long l => (MonitorStatus)(int)l,
            _ => MonitorStatus.Ok
        };

        var counters = new Dictionary<string, decimal>();
        if (countersObj is Hashtable ht)
        {
            foreach (DictionaryEntry entry in ht)
            {
                if (entry.Key is string key && decimal.TryParse(entry.Value?.ToString(), out var val))
                    counters[key] = val;
            }
        }

        return new MonitorResult(status, message, counters);
    }
}
