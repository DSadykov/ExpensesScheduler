

using ExpensesScheduler.SmtpNotificationService.Services.Models;

using MailKit.Net.Smtp;

using Microsoft.Extensions.Options;

using MimeKit;

namespace ExpensesScheduler.SmtpNotificationService.Services;

public class SmtpMessageSender(IOptions<SmtpSettings> options) : INotificationMessageSender<SmtpMessageModel>
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task SendAsync(SmtpMessageModel message, CancellationToken ct)
    {

        MimeMessage mailMessage = CreateMessage(message);

        await SendMessageAsync(mailMessage, ct);
    }

    private async Task SendMessageAsync(MimeMessage mailMessage, CancellationToken ct)
    {
        try
        {
            using var client = new SmtpClient();

            // Connect to the SMTP server
            await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSSL, ct);

            // Authenticate
            await client.AuthenticateAsync(_settings.From, _settings.Password, ct);

            await client.SendAsync(mailMessage, ct);

            Console.WriteLine("Email sent successfully!");

            // Disconnect
            client.Disconnect(true, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private MimeMessage CreateMessage(SmtpMessageModel message)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Expense scheduler", _settings.From));
        email.To.Add(new MailboxAddress(message.Name, message.To));
        email.Subject = message.Subject;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = $"<b>{message.Body}</b>"
        };

        return email;
    }
}