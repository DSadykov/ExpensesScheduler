

using ExpensesScheduler.ExpensesSchedulerService.DB.Models;

namespace ExpensesScheduler.ExpensesSchedulerService.Services;

public interface INotificationMessageSender
{
    Task<List<Guid>> SendNotificationsAsync(List<ScheduledExpenseModel> scheduledExpenses);
}