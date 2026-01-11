using ExpensesScheduler.Messaging.Consumer;
using ExpensesScheduler.Messaging.DTO;
using ExpensesScheduler.Messaging.Producer;
using ExpensesScheduler.NotificationService.Services.Models;

using Microsoft.Extensions.Options;

namespace ExpensesScheduler.NotificationService.MessageHandlers;

public class NewUserCreatedHandler(
    IMessageProducer<SmtpNewUserCreatedMessage> smtpProducer) : IMessageHandler<NewUserCreatedMessage>
{
    public async Task HandleAsync(NewUserCreatedMessage message, CancellationToken cancellationToken)
    {
        await smtpProducer.ProduceAsync(new()
        {
            Email = message.Email,
            UserID = message.UserID,
        }, cancellationToken);

    }
}
