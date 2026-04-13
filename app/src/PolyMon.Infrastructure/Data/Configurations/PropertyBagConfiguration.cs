using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolyMon.Domain.Models;

namespace PolyMon.Infrastructure.Data.Configurations;

public class PropertyBagConfiguration : IEntityTypeConfiguration<PropertyBag>
{
    public void Configure(EntityTypeBuilder<PropertyBag> builder)
    {
        builder.ToTable("PropertyBag");
        builder.HasKey(p => p.PropertyKey);
        builder.Property(p => p.PropertyKey).HasMaxLength(255);
        builder.Property(p => p.PropertyValue1).HasMaxLength(3000);
        builder.Property(p => p.PropertyValue2).HasColumnType("ntext");
    }
}
