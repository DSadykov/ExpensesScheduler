using ExpensesScheduler.ExpensesSchedulerService.DB.Models;

namespace ExpensesScheduler.ExpensesSchedulerService.DTO.Responses;

public class UpdateScheduledExpenseResponse : BaseResponse
{
    public ScheduledExpenseModel? UpdatedExpense { get;  set; }
}
