using System.Net.Mail;

namespace ExpensesScheduler.SmtpNotificationService.Services.Models;

public class SmtpMessageModel
{
    public required string To { get; set; }
    public required string Body { get;  set; }
    public required string Subject { get;  set; }
    public required string Name { get; set; }
}
