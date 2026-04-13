namespace BlazorMon.Domain.Plugins;

public class PluginDescriptor
{
    public string TypeKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ScriptPath { get; set; } = string.Empty;
    public string MetadataPath { get; set; } = string.Empty;
    public List<PluginParameter> Parameters { get; set; } = [];
}
