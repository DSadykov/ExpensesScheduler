using System.ComponentModel.DataAnnotations;
using System.Text;

using ExpensesScheduler.ExpensesSchedulerService.DB.Models;

namespace ExpensesScheduler.ExpensesSchedulerService.DTO.Requests;

public class AddScheduledExpenseRequest : IValidatableObject
{
    [Required]
    public required string Description { get; set; }
    [Required]
    public double Amount { get; set; }
    public int? HappensInDays { get; set; }
    [Required]
    public bool OneTimeOnly { get; set; }
    public ScheduleTypes? ScheduleType { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ScheduleType is null && HappensInDays is null)
        {
            yield return new("ScheduleType or HappensInDays required");
        }
        if (ScheduleType is not null && HappensInDays is not null)
        {
            yield return new("ScheduleType and HappensInDays can not be passed at the same time");
        }
    }

}
