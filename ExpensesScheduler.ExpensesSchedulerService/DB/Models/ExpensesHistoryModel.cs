using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

namespace ExpensesScheduler.ExpensesSchedulerService.DB.Models;

[Table("ExpensesHistory")]
[Index(nameof(UserId))]
[Index(nameof(DateTime))]
public class ExpensesHistoryModel
{
    [Key]
    public Guid Id { get; set; }
    [JsonIgnore]
    public Guid UserId { get; set; }
    [JsonIgnore]
    public Guid? ScheduledExpenseID { get; set; }
    public required string Description { get; set; }
    public required DateTime DateTime { get; set; }
    public required double Amount {  get; set; }
}
