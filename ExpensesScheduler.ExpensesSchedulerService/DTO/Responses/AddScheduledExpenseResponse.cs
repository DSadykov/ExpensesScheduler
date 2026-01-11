using ExpensesScheduler.ExpensesSchedulerService.DB.Models;

namespace ExpensesScheduler.ExpensesSchedulerService.DTO.Responses;

public class AddScheduledExpenseResponse : BaseResponse
{
    public ScheduledExpenseModel? CreatedExpense { get; internal set; }
}
