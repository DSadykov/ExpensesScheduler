
using System.Collections.Generic;

using ExpensesScheduler.ExpensesSchedulerService.DB;
using ExpensesScheduler.ExpensesSchedulerService.Extensions;
using ExpensesScheduler.ExpensesSchedulerService.Services.Models;
using ExpensesScheduler.Messaging.DTO;
using ExpensesScheduler.Messaging.Producer;

using Hangfire;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace ExpensesScheduler.ExpensesSchedulerService.Services.Background;

public class ScheduledExpensesWorker
    (INotificationMessageSender notificationSender,
    IDbContextFactory<ScheduledExpensesDbContext> dbContextFactory,
    IDistributedCache cache) : IHostedService, IDisposable
{
    private const string _recurringJobId = "Scheduled expenses";
    public void Dispose()
    {
        RecurringJob.RemoveIfExists(_recurringJobId);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        RecurringJob.AddOrUpdate(
            _recurringJobId,
            () => SendMessagesAsync(),
            Cron.Daily(6));
    }

    public async Task SendMessagesAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var scheduledExpenses = dbContext.GetScheduledExpenses(DateTime.UtcNow);
        var sentExpenses = await notificationSender.SendNotificationsAsync(scheduledExpenses);
        var expensesHistory = await dbContext.UpdateSentScheduledExpenses(sentExpenses);

        foreach (var expensesByUser in expensesHistory.GroupBy(x => x.UserId))
        {
            var cacheKey = cache.GetKeyForExpensesHistory(expensesByUser.Key);

            var cachedData = await cache.GetValueAsync<ExpensesHistoryCacheModel>(cacheKey);

            cachedData?.ExpensesHistoryModels.AddRange([.. expensesByUser]);
            cachedData?.End = cachedData?.End > DateTime.Now.Date ? cachedData.End : DateTime.Now.Date;

            await cache.SetValueAsync(cacheKey, cachedData);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        RecurringJob.RemoveIfExists(_recurringJobId);
    }
}
