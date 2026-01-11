using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

namespace ExpensesScheduler.ExpensesSchedulerService.DB.Models;

[Table("ScheduledExpenses")]

[Index(nameof(CreatedDate))]
[Index(nameof(LastTimeTriggered))]
[Index(nameof(UserID))]
public class ScheduledExpenseModel
{
    [Key]
    public Guid Id { get; set; }
    [JsonIgnore]
    public Guid UserID { get; set; }

    public required string Description { get; set; }
    public double Amount { get; set; }
    public DateTime CreatedDate { get; set; }

    [JsonIgnore]
    public DateTime? LastTimeTriggered { get; set; }
    public int? HappensInDays{ get; set; }
    public bool OneTimeOnly { get; set; } = false;
    public bool? EveryMonth { get; set; }

}
public enum ScheduleTypes
{
    PerDay,
    PerWeek,
    PerMonth
}