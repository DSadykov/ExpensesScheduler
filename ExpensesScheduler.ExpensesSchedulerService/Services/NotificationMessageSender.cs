using ExpensesScheduler.ExpensesSchedulerService.DB;
using ExpensesScheduler.ExpensesSchedulerService.DB.Models;
using ExpensesScheduler.Messaging.DTO;
using ExpensesScheduler.Messaging.Producer;

namespace ExpensesScheduler.ExpensesSchedulerService.Services;

public class NotificationMessageSender(
    IMessageProducer<NotificationMessage> notificationProducer,
    ILogger<NotificationMessageSender> logger) : INotificationMessageSender
{
    public async Task<List<Guid>> SendNotificationsAsync(List<ScheduledExpenseModel> scheduledExpenses)
    {
        var result = new List<Guid>();
        foreach (var schedule in scheduledExpenses)
        {
            try
            {
                await notificationProducer.ProduceAsync(new()
                {
                    Amount = schedule.Amount,
                    Message = schedule.Description,
                    UserID = schedule.UserID.ToString()
                }, default);
                result.Add(schedule.Id);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"Error while sending message to kafka with scheduled expense id {schedule.Id}");
            }
        }
        return result;
    }
}
