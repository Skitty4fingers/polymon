using System.Text.Json;
using Microsoft.Extensions.Logging;
using BlazorMon.Application.Interfaces;
using BlazorMon.Domain.Plugins;

namespace BlazorMon.Infrastructure.Plugins;

public class PluginScanner(string pluginsDirectory, ILogger<PluginScanner> logger) : IPluginScanner
{
    private List<PluginDescriptor> _descriptors = [];

    public IReadOnlyList<PluginDescriptor> Descriptors => _descriptors;

    public PluginDescriptor? GetByKey(string typeKey)
        => _descriptors.FirstOrDefault(d => string.Equals(d.TypeKey, typeKey, StringComparison.OrdinalIgnoreCase));

    public void Reload()
    {
        if (!Directory.Exists(pluginsDirectory))
        {
            logger.LogWarning("Plugins directory '{Directory}' not found", pluginsDirectory);
            _descriptors = [];
            return;
        }

        var loaded = new List<PluginDescriptor>();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        foreach (var metaFile in Directory.GetFiles(pluginsDirectory, "*.plugin.json"))
        {
            try
            {
                var json = File.ReadAllText(metaFile);
                var descriptor = JsonSerializer.Deserialize<PluginDescriptor>(json, options);
                if (descriptor is null) continue;

                descriptor.MetadataPath = metaFile;
                var scriptFile = Path.Combine(pluginsDirectory, $"{descriptor.TypeKey.ToLowerInvariant()}.ps1");
                if (!File.Exists(scriptFile))
                {
                    logger.LogWarning("Script file not found for plugin '{TypeKey}': {ScriptFile}", descriptor.TypeKey, scriptFile);
                    continue;
                }

                descriptor.ScriptPath = scriptFile;
                loaded.Add(descriptor);
                logger.LogDebug("Loaded plugin '{TypeKey}' from {MetaFile}", descriptor.TypeKey, metaFile);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load plugin metadata from {MetaFile}", metaFile);
            }
        }

        _descriptors = loaded;
        logger.LogInformation("Loaded {Count} plugins from {Directory}", loaded.Count, pluginsDirectory);
    }
}
