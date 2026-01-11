using System.Collections.Specialized;
using System.Web;

using ExpensesScheduler.Messaging.Consumer;
using ExpensesScheduler.Messaging.DTO;
using ExpensesScheduler.SmtpNotificationService.DB;
using ExpensesScheduler.SmtpNotificationService.Services;
using ExpensesScheduler.SmtpNotificationService.Services.Models;

using Microsoft.EntityFrameworkCore;

namespace ExpensesScheduler.SmtpNotificationService.MessageHandlers;

public class SmtpNewUserCreatedHandler(
    IDbContextFactory<SmtpNotificationSettingsDbContext> settingsDbContextFactory,
    INotificationMessageSender<SmtpMessageModel> messageSender) : IMessageHandler<SmtpNewUserCreatedMessage>
{
    public async Task HandleAsync(SmtpNewUserCreatedMessage message, CancellationToken cancellationToken)
    {
        await using var dbContext = settingsDbContextFactory.CreateDbContext();

        await dbContext.AddNewUserAsync(message.UserID, message.Email);

        await messageSender.SendAsync(new()
        {
            Body = $"Необходимо подтвердить почту перейдя по ссылке {BuildConfirmLink(message)}",
            Name = message.Email,
            Subject = $"Подтверждение почты для Expense Scheduler",
            To = message.Email,
        }, cancellationToken);
    }

    private static string BuildConfirmLink(SmtpNewUserCreatedMessage message)
    {
        var urlBuilder = new UriBuilder()
        {
            Host = "localhost",
            Port = 8082,
            Scheme = "https",
        };

        urlBuilder.Path += "/Authorization/ConfirmEmail";

        NameValueCollection query = HttpUtility.ParseQueryString(urlBuilder.Query);

        query["UserID"] = message.UserID.ToString();

        urlBuilder.Query = query.ToString();
        return urlBuilder.ToString();
    }
}
