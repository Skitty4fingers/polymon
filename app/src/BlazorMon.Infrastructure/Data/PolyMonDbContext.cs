using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlazorMon.Domain.Models;
using BlazorMon.Infrastructure.Identity;

namespace BlazorMon.Infrastructure.Data;

public class PolyMonDbContext(DbContextOptions<PolyMonDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
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
        base.OnModelCreating(modelBuilder);  // Identity tables first
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PolyMonDbContext).Assembly);
    }
}
