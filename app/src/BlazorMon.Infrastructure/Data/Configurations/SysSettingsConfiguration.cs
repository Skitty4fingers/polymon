using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BlazorMon.Domain.Models;

namespace BlazorMon.Infrastructure.Data.Configurations;

public class SysSettingsConfiguration : IEntityTypeConfiguration<SysSettings>
{
    public void Configure(EntityTypeBuilder<SysSettings> builder)
    {
        builder.ToTable("SysSettings");
        builder.HasKey(s => s.Name);
        builder.Property(s => s.Name).HasMaxLength(50);
        builder.Property(s => s.ServiceServer).HasMaxLength(255);
        builder.Property(s => s.MainTimerInterval).HasDefaultValue(60000);
        builder.Property(s => s.SmtpFromName).HasColumnName("SMTPFromName").HasMaxLength(50);
        builder.Property(s => s.SmtpFromAddress).HasColumnName("SMTPFromAddress").HasMaxLength(255);
        builder.Property(s => s.UseInternalSmtp).HasColumnName("UseInternalSMTP");
        builder.Property(s => s.ExtSmtpServer).HasColumnName("ExtSMTPServer").HasMaxLength(255);
        builder.Property(s => s.ExtSmtpPort).HasColumnName("ExtSMTPPort");
        builder.Property(s => s.ExtSmtpUserId).HasColumnName("ExtSMTPUserID").HasMaxLength(50);
        builder.Property(s => s.ExtSmtpPwd).HasColumnName("ExtSMTPPwd").HasMaxLength(50);
        builder.Property(s => s.ExtSmtpUseSsl).HasColumnName("ExtSMTPUseSSL");
        builder.Property(s => s.RetentionMaxMonthsRaw).HasDefaultValue((short)24);
        builder.Property(s => s.RetentionMaxMonthsDaily).HasDefaultValue((short)36);
        builder.Property(s => s.RetentionMaxMonthsWeekly).HasDefaultValue((short)60);
        builder.Property(s => s.RetentionMaxMonthsMonthly).HasDefaultValue((short)60);
    }
}
