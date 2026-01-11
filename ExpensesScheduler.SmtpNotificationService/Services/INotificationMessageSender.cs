namespace ExpensesScheduler.SmtpNotificationService.Services;

public interface INotificationMessageSender<MessageType>
{
    Task SendAsync(MessageType messageType, CancellationToken ct);
}