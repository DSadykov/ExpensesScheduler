using ExpensesScheduler.ExpensesSchedulerService.DTO.Requests;
using ExpensesScheduler.ExpensesSchedulerService.DTO.Responses;

namespace ExpensesScheduler.ExpensesSchedulerService.Services;

public interface ISheduledExpensesService
{
    Task<AddScheduledExpenseResponse> AddScheduledExpenseAsync(AddScheduledExpenseRequest request, Guid userID);
    Task<BaseResponse> DeleteScheduledExpenseAsync(DeleteScheduledExpenseRequest request, Guid userID);
    Task<GetExpensesHistoryResponse> GetExpensesHistoryAsync(Guid userID, DateTime start, DateTime end);
    Task<ScheduledExpensesResponse> GetScheduledExpensesByUserIDAsync(Guid userID);
    Task<UpdateScheduledExpenseResponse> UpdateScheduledExpenseAsync(UpdateScheduledExpenseRequest request, Guid userID);
}