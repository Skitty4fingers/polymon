using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BlazorMon.Domain.Models;

namespace BlazorMon.Infrastructure.Data.Configurations;

public class MonitorEventConfiguration : IEntityTypeConfiguration<MonitorEvent>
{
    public void Configure(EntityTypeBuilder<MonitorEvent> builder)
    {
        builder.ToTable("MonitorEvent");
        builder.HasKey(e => e.EventId);
        builder.Property(e => e.EventId).HasColumnName("EventID");
        builder.Property(e => e.MonitorId).HasColumnName("MonitorID");
        builder.Property(e => e.StatusId).HasColumnName("StatusID");
        builder.Property(e => e.EventDt).HasColumnName("EventDT");
        builder.Property(e => e.Message).HasMaxLength(500);

        builder.HasOne(e => e.Monitor)
               .WithMany(m => m.Events)
               .HasForeignKey(e => e.MonitorId);
    }
}
