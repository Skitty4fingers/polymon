using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BlazorMon.Domain.Models;

namespace BlazorMon.Infrastructure.Data.Configurations;

public class DashboardGroupConfiguration : IEntityTypeConfiguration<DashboardGroup>
{
    public void Configure(EntityTypeBuilder<DashboardGroup> builder)
    {
        builder.ToTable("DashboardGroup");
        builder.HasKey(g => g.GroupId);
        builder.Property(g => g.GroupId).HasColumnName("GroupID");
        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
    }
}

public class DashboardGroupMonitorConfiguration : IEntityTypeConfiguration<DashboardGroupMonitor>
{
    public void Configure(EntityTypeBuilder<DashboardGroupMonitor> builder)
    {
        builder.ToTable("DashboardGroupMonitor");
        builder.HasKey(gm => new { gm.GroupId, gm.MonitorId });
        builder.Property(gm => gm.GroupId).HasColumnName("GroupID");
        builder.Property(gm => gm.MonitorId).HasColumnName("MonitorID");

        builder.HasOne(gm => gm.Group)
               .WithMany(g => g.GroupMonitors)
               .HasForeignKey(gm => gm.GroupId);
    }
}
