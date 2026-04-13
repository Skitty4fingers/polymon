using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BlazorMon.Domain.Models;

namespace BlazorMon.Infrastructure.Data.Configurations;

public class OperatorConfiguration : IEntityTypeConfiguration<Operator>
{
    public void Configure(EntityTypeBuilder<Operator> builder)
    {
        builder.ToTable("Operator");
        builder.HasKey(o => o.OperatorId);
        builder.Property(o => o.OperatorId).HasColumnName("OperatorID");
        builder.Property(o => o.Name).HasMaxLength(255).IsRequired();
        builder.Property(o => o.EmailAddress).HasMaxLength(255).IsRequired();
        builder.Property(o => o.OfflineTimeStart).HasMaxLength(5).IsFixedLength().HasDefaultValue("00:00");
        builder.Property(o => o.OfflineTimeEnd).HasMaxLength(5).IsFixedLength().HasDefaultValue("00:00");
        builder.Property(o => o.SummaryNotifyTime).HasMaxLength(5).IsFixedLength().HasDefaultValue("08:00");
        builder.Property(o => o.SummaryNextNotifyDt).HasColumnName("SummaryNextNotifyDT").HasDefaultValueSql("getdate()");
    }
}
