namespace PolyMon.Domain.Plugins;

public class PluginParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";   // string | decimal | int | bool
    public object? Default { get; set; }
    public bool Required { get; set; }
    public string Description { get; set; } = string.Empty;
}
