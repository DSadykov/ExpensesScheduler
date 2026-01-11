using System.Security;

namespace ExpensesScheduler.SmtpNotificationService.Services.Models;

public class SmtpSettings
{
    public int Port { get; set; }
    public required string Host { get; set; }
    public required string From { get; set; }
    public required string Password { get; set; }
    public bool UseSSL {  get; set; }
}