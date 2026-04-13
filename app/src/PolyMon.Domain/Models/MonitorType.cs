namespace PolyMon.Domain.Models;

public class MonitorType
{
    public int MonitorTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TypeKey { get; set; } = string.Empty;   // matches plugin TypeKey
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;

    public ICollection<Monitor> Monitors { get; set; } = [];
}
