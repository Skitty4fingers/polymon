using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using BlazorMon.Application.Interfaces;
using BlazorMon.Domain.Models;

namespace BlazorMon.Infrastructure.Services;

public class EmailNotificationService(
    ISysSettingsRepository settingsRepo,
    ILogger<EmailNotificationService> logger) : IEmailNotificationService
{
    public async Task SendAlertAsync(MonitorEvent monitorEvent, MonitorDef monitor, IEnumerable<Operator> operators, CancellationToken ct = default)
    {
        var settings = await settingsRepo.GetAsync(ct);
        if (settings is null) return;

        var statusLabel = ((Domain.Enums.MonitorStatus)monitorEvent.StatusId).ToString();
        var subject = BuildTemplate(monitor.MessageSubjectTemplate, monitor, monitorEvent, statusLabel);
        var body = monitor.MessageBodyTemplate is not null
            ? BuildTemplate(monitor.MessageBodyTemplate, monitor, monitorEvent, statusLabel)
            : $"Monitor: {monitor.Name}\nStatus: {statusLabel}\nMessage: {monitorEvent.Message}\nTime: {monitorEvent.EventDt:u}";

        foreach (var op in operators)
        {
            try
            {
                await SendEmailAsync(settings, op.EmailAddress, op.Name, subject,
                    op.IncludeMessageBody ? body : $"Status: {statusLabel} - {monitor.Name}", ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send alert to {Operator} ({Email})", op.Name, op.EmailAddress);
            }
        }
    }

    public async Task SendSummaryAsync(Operator op, IEnumerable<MonitorEvent> recentEvents, CancellationToken ct = default)
    {
        var settings = await settingsRepo.GetAsync(ct);
        if (settings is null) return;

        var events = recentEvents.ToList();
        var lines = events.Select(e =>
            $"[{((Domain.Enums.MonitorStatus)e.StatusId)}] {e.Monitor?.Name ?? $"Monitor {e.MonitorId}"}: {e.Message} ({e.EventDt:u})");

        var body = $"BlazorMon Daily Summary\n\nEvents in last 24 hours: {events.Count}\n\n" +
                   string.Join("\n", lines);

        try
        {
            await SendEmailAsync(settings, op.EmailAddress, op.Name, "BlazorMon Daily Summary", body, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send summary to {Operator}", op.Name);
        }
    }

    private async Task SendEmailAsync(SysSettings settings, string toAddress, string toName,
        string subject, string body, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.SmtpFromName, settings.SmtpFromAddress));
        message.To.Add(new MailboxAddress(toName, toAddress));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();

        if (settings.UseInternalSmtp)
        {
            await client.ConnectAsync("localhost", 25, SecureSocketOptions.None, ct);
        }
        else
        {
            var sslOption = settings.ExtSmtpUseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(settings.ExtSmtpServer!, settings.ExtSmtpPort ?? 25, sslOption, ct);

            if (!string.IsNullOrEmpty(settings.ExtSmtpUserId))
                await client.AuthenticateAsync(settings.ExtSmtpUserId, settings.ExtSmtpPwd, ct);
        }

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        logger.LogInformation("Email sent to {ToAddress}: {Subject}", toAddress, subject);
    }

    private static string BuildTemplate(string template, MonitorDef monitor, MonitorEvent evt, string statusLabel)
    {
        return template
            .Replace("{MonitorName}", monitor.Name)
            .Replace("{Status}", statusLabel)
            .Replace("{Message}", evt.Message ?? string.Empty)
            .Replace("{EventDT}", evt.EventDt.ToString("u"));
    }
}
