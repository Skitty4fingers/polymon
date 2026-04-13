using BlazorMon.Domain.Plugins;

namespace BlazorMon.Application.Interfaces;

public interface IPluginScanner
{
    IReadOnlyList<PluginDescriptor> Descriptors { get; }
    PluginDescriptor? GetByKey(string typeKey);
    void Reload();
}
