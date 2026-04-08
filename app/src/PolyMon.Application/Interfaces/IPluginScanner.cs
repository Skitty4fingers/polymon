using PolyMon.Domain.Plugins;

namespace PolyMon.Application.Interfaces;

public interface IPluginScanner
{
    IReadOnlyList<PluginDescriptor> Descriptors { get; }
    PluginDescriptor? GetByKey(string typeKey);
    void Reload();
}
