using System.ComponentModel.DataAnnotations;

using ExpensesScheduler.ExpensesSchedulerService.DB.Models;

namespace ExpensesScheduler.ExpensesSchedulerService.DTO.Requests;

public class UpdateScheduledExpenseRequest : IValidatableObject
{
    [Required]
    public required Guid ExpenseID {  get; set; }
    public required string Description { get; set; }
    public double Amount { get; set; }
    public int? HappensInDays { get; set; }
    public bool? OneTimeOnly { get; set; }
    public ScheduleTypes? ScheduleType { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ScheduleType is not null && HappensInDays is not null)
        {
            yield return new("ScheduleType and HappensInDays can not be passed at the same time");
        }
    }
}
