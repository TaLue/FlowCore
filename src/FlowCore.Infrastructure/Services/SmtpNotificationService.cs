using System.Net;
using System.Net.Mail;
using FlowCore.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowCore.Infrastructure.Services;

public class SmtpNotificationService : INotificationService
{
    private readonly ILogger<SmtpNotificationService> _logger;
    private readonly SmtpSettings _settings;

    public SmtpNotificationService(ILogger<SmtpNotificationService> logger, IOptions<SmtpSettings> options)
    {
        _logger = logger;
        _settings = options.Value;
    }

    public async Task NotifyApproverAsync(string approverEmail, string requestTitle, int requestId)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("[NOTIFICATION] Approver {Email} – New request pending: '{Title}' (ID: {Id})",
                approverEmail, requestTitle, requestId);
            return;
        }

        var subject = $"[FlowCore] คำขออนุมัติใหม่: {requestTitle}";
        var body = $"""
            มีคำขออนุมัติใหม่รอการพิจารณา

            หัวข้อ  : {requestTitle}
            รหัสคำขอ: {requestId}

            กรุณาเข้าสู่ระบบ FlowCore เพื่อดำเนินการ
            """;

        await SendAsync(approverEmail, subject, body);
    }

    public async Task NotifyRequesterAsync(string requesterEmail, string requestTitle, string action)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("[NOTIFICATION] Requester {Email} – '{Title}' {Action}",
                requesterEmail, requestTitle, action);
            return;
        }

        var actionText = action switch
        {
            "Approve" => "ได้รับการอนุมัติแล้ว",
            "Reject"  => "ถูกปฏิเสธ",
            "Return"  => "ถูกส่งกลับเพื่อแก้ไข",
            _         => action
        };

        var subject = $"[FlowCore] อัปเดตคำขอของคุณ: {requestTitle}";
        var body = $"""
            คำขอของคุณ "{requestTitle}" {actionText}

            กรุณาเข้าสู่ระบบ FlowCore เพื่อดูรายละเอียดเพิ่มเติม
            """;

        await SendAsync(requesterEmail, subject, body);
    }

    private async Task SendAsync(string to, string subject, string body)
    {
        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 10_000
            };

            var from = new MailAddress(_settings.FromAddress, _settings.FromName);
            using var message = new MailMessage(from, new MailAddress(to))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            // Log and swallow — notification failure must not break approval flow
            _logger.LogError(ex, "Failed to send email to {To}: {Subject}", to, subject);
        }
    }
}
