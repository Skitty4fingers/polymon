using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolyMon.Domain.Models;

namespace PolyMon.Infrastructure.Data.Configurations;

public class MonitorEventCounterConfiguration : IEntityTypeConfiguration<MonitorEventCounter>
{
    public void Configure(EntityTypeBuilder<MonitorEventCounter> builder)
    {
        builder.ToTable("MonitorEventCounter");
        builder.HasKey(c => c.CounterId);
        builder.Property(c => c.CounterId).HasColumnName("CounterID");
        builder.Property(c => c.EventId).HasColumnName("EventID");
        builder.Property(c => c.MonitorId).HasColumnName("MonitorID");
        builder.Property(c => c.CounterName).HasMaxLength(255);
        builder.Property(c => c.CounterValue).HasColumnType("decimal(30,10)");
        builder.Property(c => c.EventDt).HasColumnName("EventDT");

        builder.HasOne(c => c.Event)
               .WithMany(e => e.Counters)
               .HasForeignKey(c => c.EventId);
    }
}
