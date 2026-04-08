using Microsoft.EntityFrameworkCore;
using PolyMon.Domain.Models;

namespace PolyMon.Infrastructure.Data;

public class PolyMonDbContext(DbContextOptions<PolyMonDbContext> options) : DbContext(options)
{
    public DbSet<MonitorDef> Monitors => Set<MonitorDef>();
    public DbSet<MonitorType> MonitorTypes => Set<MonitorType>();
    public DbSet<MonitorEvent> MonitorEvents => Set<MonitorEvent>();
    public DbSet<MonitorEventCounter> MonitorEventCounters => Set<MonitorEventCounter>();
    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<MonitorOperator> MonitorOperators => Set<MonitorOperator>();
    public DbSet<SysSettings> SysSettings => Set<SysSettings>();
    public DbSet<DashboardGroup> DashboardGroups => Set<DashboardGroup>();
    public DbSet<DashboardGroupMonitor> DashboardGroupMonitors => Set<DashboardGroupMonitor>();
    public DbSet<PropertyBag> PropertyBag => Set<PropertyBag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PolyMonDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
