using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BlazorMon.Domain.Models;

namespace BlazorMon.Infrastructure.Data.Configurations;

public class MonitorConfiguration : IEntityTypeConfiguration<MonitorDef>
{
    public void Configure(EntityTypeBuilder<MonitorDef> builder)
    {
        builder.ToTable("Monitor");
        builder.HasKey(m => m.MonitorId);
        builder.Property(m => m.MonitorId).HasColumnName("MonitorID");
        builder.Property(m => m.Name).HasMaxLength(50).IsRequired();
        builder.Property(m => m.MonitorTypeId).HasColumnName("MonitorTypeID");
        builder.Property(m => m.MonitorXml).HasColumnName("MonitorXML").HasColumnType("ntext");
        builder.Property(m => m.MessageSubjectTemplate).HasMaxLength(100);
        builder.Property(m => m.MessageBodyTemplate).HasMaxLength(3000);
        builder.Property(m => m.OfflineTime1Start).HasMaxLength(5).HasDefaultValue("00:00");
        builder.Property(m => m.OfflineTime1End).HasMaxLength(5).HasDefaultValue("00:00");
        builder.Property(m => m.OfflineTime2Start).HasMaxLength(5).HasDefaultValue("00:00");
        builder.Property(m => m.OfflineTime2End).HasMaxLength(5).HasDefaultValue("00:00");
        builder.Property(m => m.AfterEventScriptEngineId).HasColumnName("AfterEventScriptEngineID");
        builder.Property(m => m.AfterEventScript).HasColumnType("ntext");

        builder.HasOne(m => m.MonitorType)
               .WithMany(mt => mt.Monitors)
               .HasForeignKey(m => m.MonitorTypeId);
    }
}
