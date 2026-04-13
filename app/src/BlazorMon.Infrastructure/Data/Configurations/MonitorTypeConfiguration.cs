using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BlazorMon.Domain.Models;

namespace BlazorMon.Infrastructure.Data.Configurations;

public class MonitorTypeConfiguration : IEntityTypeConfiguration<MonitorType>
{
    public void Configure(EntityTypeBuilder<MonitorType> builder)
    {
        builder.ToTable("MonitorType");
        builder.HasKey(mt => mt.MonitorTypeId);
        builder.Property(mt => mt.MonitorTypeId).HasColumnName("MonitorTypeID");
        builder.Property(mt => mt.Name).HasMaxLength(100).IsRequired();
        builder.Property(mt => mt.TypeKey).HasMaxLength(50).IsRequired();
        builder.Property(mt => mt.Description).HasMaxLength(500);
    }
}
