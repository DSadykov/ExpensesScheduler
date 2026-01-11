using ExpensesScheduler.ExpensesSchedulerService.DB.Models;

namespace ExpensesScheduler.ExpensesSchedulerService.DTO.Responses;

public class GetExpensesHistoryResponse : BaseResponse
{
    public Guid? UserID {  get; set; }
    public List<ExpensesHistoryModel>? ExpensesHistory { get; set; }
}
