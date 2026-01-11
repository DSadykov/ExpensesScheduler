using ExpensesScheduler.Messaging.Consumer;
using ExpensesScheduler.Messaging.DTO;
using ExpensesScheduler.Messaging.Producer;
using ExpensesScheduler.NotificationService.Services.Models;

using Microsoft.Extensions.Options;

namespace ExpensesScheduler.NotificationService.MessageHandlers;

public class NotificationHandler(
    IMessageProducer<SmtpNotificationMessage> smtpProducer,
    IOptions<NotificationSettings> options) : IMessageHandler<NotificationMessage>
{
    public async Task HandleAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        if (options.Value.SmtpEnabled)
        {
            await smtpProducer.ProduceAsync(new()
            {
                Amount = message.Amount,
                Message = message.Message,
                UserID = message.UserID,
            }, cancellationToken);
        }
    }
}
