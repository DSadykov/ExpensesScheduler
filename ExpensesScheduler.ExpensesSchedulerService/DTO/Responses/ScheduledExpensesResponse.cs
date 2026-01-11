using ExpensesScheduler.ExpensesSchedulerService.DB.Models;

namespace ExpensesScheduler.ExpensesSchedulerService.DTO.Responses;

public class ScheduledExpensesResponse : BaseResponse
{
    public Guid? UserID {  get; set; }
    public List<ScheduledExpenseModel>? ScheduledExpenses { get; set; }
}
