using ExpensesScheduler.Messaging.Consumer;
using ExpensesScheduler.Messaging.DTO;
using ExpensesScheduler.SmtpNotificationService.DB;
using ExpensesScheduler.SmtpNotificationService.Services;
using ExpensesScheduler.SmtpNotificationService.Services.Models;

using Microsoft.EntityFrameworkCore;

namespace ExpensesScheduler.SmtpNotificationService.MessageHandlers;

public class SmtpNotificationHandler(
    INotificationMessageSender<SmtpMessageModel> smtpSender,
    IDbContextFactory<SmtpNotificationSettingsDbContext> settingsDbContextFactory) : IMessageHandler<NotificationMessage>
{
    public async Task HandleAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        await using var dbContext = settingsDbContextFactory.CreateDbContext();

        var userSettings = dbContext.NotificationSettings.Find(Guid.Parse(message.UserID));

        if (userSettings is null || !userSettings.IsActive)
        {
            return;
        }

        await smtpSender.SendAsync(new()
        {
            Name = userSettings.Email,
            Subject = message.Message,
            To = userSettings.Email,
            Body = $"У вас сегодня запланирована трата в размере {message.Amount} " +
            $"по позиции {message.Message}"
        }, cancellationToken);
    }
}
