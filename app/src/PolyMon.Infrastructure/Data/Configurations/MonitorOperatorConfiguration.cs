using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolyMon.Domain.Models;

namespace PolyMon.Infrastructure.Data.Configurations;

public class MonitorOperatorConfiguration : IEntityTypeConfiguration<MonitorOperator>
{
    public void Configure(EntityTypeBuilder<MonitorOperator> builder)
    {
        builder.ToTable("MonitorOperator");
        builder.HasKey(mo => new { mo.MonitorId, mo.OperatorId });
        builder.Property(mo => mo.MonitorId).HasColumnName("MonitorID");
        builder.Property(mo => mo.OperatorId).HasColumnName("OperatorID");

        builder.HasOne(mo => mo.Monitor)
               .WithMany(m => m.MonitorOperators)
               .HasForeignKey(mo => mo.MonitorId);

        builder.HasOne(mo => mo.Operator)
               .WithMany(o => o.MonitorOperators)
               .HasForeignKey(mo => mo.OperatorId);
    }
}
