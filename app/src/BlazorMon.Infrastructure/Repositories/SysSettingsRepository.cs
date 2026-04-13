using Microsoft.EntityFrameworkCore;
using BlazorMon.Application.Interfaces;
using BlazorMon.Domain.Models;
using BlazorMon.Infrastructure.Data;

namespace BlazorMon.Infrastructure.Repositories;

public class SysSettingsRepository(PolyMonDbContext db) : ISysSettingsRepository
{
    public Task<SysSettings?> GetAsync(CancellationToken ct = default)
        => db.SysSettings.FirstOrDefaultAsync(ct);

    public async Task SaveAsync(SysSettings settings, CancellationToken ct = default)
    {
        var existing = await db.SysSettings.FirstOrDefaultAsync(ct);
        if (existing is null)
            db.SysSettings.Add(settings);
        else
        {
            existing.Name = settings.Name;
            existing.IsEnabled = settings.IsEnabled;
            existing.ServiceServer = settings.ServiceServer;
            existing.MainTimerInterval = settings.MainTimerInterval;
            existing.UseInternalSmtp = settings.UseInternalSmtp;
            existing.SmtpFromName = settings.SmtpFromName;
            existing.SmtpFromAddress = settings.SmtpFromAddress;
            existing.ExtSmtpServer = settings.ExtSmtpServer;
            existing.ExtSmtpPort = settings.ExtSmtpPort;
            existing.ExtSmtpUserId = settings.ExtSmtpUserId;
            existing.ExtSmtpPwd = settings.ExtSmtpPwd;
            existing.ExtSmtpUseSsl = settings.ExtSmtpUseSsl;
            existing.RetentionMaxMonthsRaw = settings.RetentionMaxMonthsRaw;
            existing.RetentionMaxMonthsDaily = settings.RetentionMaxMonthsDaily;
            existing.RetentionMaxMonthsWeekly = settings.RetentionMaxMonthsWeekly;
            existing.RetentionMaxMonthsMonthly = settings.RetentionMaxMonthsMonthly;
        }
        await db.SaveChangesAsync(ct);
    }
}
