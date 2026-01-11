using System;

using ExpensesScheduler.ExpensesSchedulerService.DB.Models;

using Microsoft.EntityFrameworkCore;

namespace ExpensesScheduler.ExpensesSchedulerService.DB;

public class ScheduledExpensesDbContext : DbContext
{

    public ScheduledExpensesDbContext(DbContextOptions options)
        : base(options)
    {
        Database.EnsureCreated();
    }
    public DbSet<ExpensesHistoryModel> ExpensesHistory { get; set; }
    public DbSet<ScheduledExpenseModel> ScheduledExpenses { get; set; }


    public Dictionary<Guid,ScheduledExpenseModel> GetScheduledExpensesByUserID(Guid userID)
    {
        return ScheduledExpenses.Where(x => x.UserID == userID)
            .AsNoTracking()
            .ToDictionary(x=>x.Id);
    }

    public List<ScheduledExpenseModel> GetScheduledExpenses(DateTime dateTime)
    {
        return [.. ScheduledExpenses
            .Where(x => x.EveryMonth == true
            ? (x.LastTimeTriggered ?? x.CreatedDate).AddMonths(1) <= dateTime
            : (x.LastTimeTriggered ?? x.CreatedDate).AddDays(x.HappensInDays.Value) <= dateTime)];
    }

    public async Task<List<ExpensesHistoryModel>> UpdateSentScheduledExpenses(List<Guid> ids)
    {
        var sentSchedules =
            ScheduledExpenses
            .Where(x => ids.Contains(x.Id));
        var result = new List<ExpensesHistoryModel>();
        foreach (var schedule in sentSchedules)
        {
            ExpensesHistoryModel entity = new()
            {
                Amount = schedule.Amount,
                DateTime = DateTime.UtcNow,
                //Main schedule will be deleted at the same time, so no point in writing its id
                ScheduledExpenseID = schedule.OneTimeOnly ? null : schedule.Id,
                Id = Guid.NewGuid(),
                UserId = schedule.UserID,
                Description = schedule.Description,
            };
            ExpensesHistory.Add(entity);
            result.Add(entity);
            if (schedule.OneTimeOnly)
            {
                ScheduledExpenses.Remove(schedule);
            }
            else
            {
                schedule.LastTimeTriggered = DateTime.UtcNow.Date;
            }
        }
        try
        {
            await SaveChangesAsync();
            return result;
        }
        catch (Exception ex)
        {
            ChangeTracker.Clear();
            return [];
        }
    }

    public List<ExpensesHistoryModel> GetExpensesHistoryModels(Guid userID, DateTime from, DateTime to)
    {
        return [.. ExpensesHistory.Where(x =>
                 x.UserId == userID
                 && x.DateTime.Date >= from.Date && x.DateTime.Date <= to.Date)
                .AsNoTracking()];
    }


}
