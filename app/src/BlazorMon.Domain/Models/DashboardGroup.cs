namespace BlazorMon.Domain.Models;

public class DashboardGroup
{
    public int GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public ICollection<DashboardGroupMonitor> GroupMonitors { get; set; } = [];
}

public class DashboardGroupMonitor
{
    public int GroupId { get; set; }
    public int MonitorId { get; set; }
    public int DisplayOrder { get; set; }

    public DashboardGroup? Group { get; set; }
    public Monitor? Monitor { get; set; }
}
