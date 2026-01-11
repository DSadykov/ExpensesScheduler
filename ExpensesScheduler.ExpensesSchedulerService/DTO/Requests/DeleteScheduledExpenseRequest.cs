using System.ComponentModel.DataAnnotations;

namespace ExpensesScheduler.ExpensesSchedulerService.DTO.Requests;

public class DeleteScheduledExpenseRequest
{
    [Required]
    public Guid ExpenseID {  get; set; }

}
