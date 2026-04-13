namespace BlazorMon.Domain.Models;

public class SysSettings
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string ServiceServer { get; set; } = string.Empty;
    public int MainTimerInterval { get; set; } = 60;   // seconds
    public bool UseInternalSmtp { get; set; } = true;
    public string SmtpFromName { get; set; } = string.Empty;
    public string SmtpFromAddress { get; set; } = string.Empty;
    public string? ExtSmtpServer { get; set; }
    public int? ExtSmtpPort { get; set; }
    public string? ExtSmtpUserId { get; set; }
    public string? ExtSmtpPwd { get; set; }
    public bool ExtSmtpUseSsl { get; set; }

    // Global retention defaults (months)
    public short RetentionMaxMonthsRaw { get; set; } = 24;
    public short RetentionMaxMonthsDaily { get; set; } = 36;
    public short RetentionMaxMonthsWeekly { get; set; } = 60;
    public short RetentionMaxMonthsMonthly { get; set; } = 60;

    public decimal DbVersion { get; set; }
}
