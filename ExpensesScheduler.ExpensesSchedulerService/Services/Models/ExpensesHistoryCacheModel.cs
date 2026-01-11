using ExpensesScheduler.ExpensesSchedulerService.DB.Models;

namespace ExpensesScheduler.ExpensesSchedulerService.Services.Models;

public class ExpensesHistoryCacheModel
{
    public required DateTime Start { get; set; }
    public required DateTime End { get; set; }
    public required List<ExpensesHistoryModel> ExpensesHistoryModels { get; set; }
}
