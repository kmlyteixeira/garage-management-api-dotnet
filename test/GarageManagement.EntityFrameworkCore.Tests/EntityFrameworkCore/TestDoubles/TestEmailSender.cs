using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Volo.Abp.Emailing;

namespace GarageManagement.EntityFrameworkCore.TestDoubles;

public class TestEmailSender : IEmailSender
{
    private readonly ConcurrentBag<SentEmailMessage> sentEmails = new();

    public IReadOnlyCollection<SentEmailMessage> SentEmails => sentEmails.ToArray();

    public Task SendAsync(string to, string? subject, string? body, bool isBodyHtml = true, AdditionalEmailSendingArgs? additionalEmailSendingArgs = null)
    {
        sentEmails.Add(new SentEmailMessage(to, subject ?? string.Empty, body ?? string.Empty, isBodyHtml));
        return Task.CompletedTask;
    }

    public Task SendAsync(string from, string to, string? subject, string? body, bool isBodyHtml = true, AdditionalEmailSendingArgs? additionalEmailSendingArgs = null)
    {
        sentEmails.Add(new SentEmailMessage(to, subject ?? string.Empty, body ?? string.Empty, isBodyHtml));
        return Task.CompletedTask;
    }

    public Task SendAsync(MailMessage mail, bool normalize = true)
    {
        var to = mail.To.Count > 0 ? mail.To[0].Address : string.Empty;
        sentEmails.Add(new SentEmailMessage(to, mail.Subject ?? string.Empty, mail.Body ?? string.Empty, mail.IsBodyHtml));
        return Task.CompletedTask;
    }

    public Task QueueAsync(string to, string subject, string body, bool isBodyHtml = true, AdditionalEmailSendingArgs? additionalEmailSendingArgs = null)
    {
        sentEmails.Add(new SentEmailMessage(to, subject, body, isBodyHtml));
        return Task.CompletedTask;
    }

    public Task QueueAsync(string from, string to, string subject, string body, bool isBodyHtml = true, AdditionalEmailSendingArgs? additionalEmailSendingArgs = null)
    {
        sentEmails.Add(new SentEmailMessage(to, subject, body, isBodyHtml));
        return Task.CompletedTask;
    }

    public void Clear()
    {
        while (sentEmails.TryTake(out _))
        {
        }
    }
}

public record SentEmailMessage(string To, string Subject, string Body, bool IsBodyHtml);