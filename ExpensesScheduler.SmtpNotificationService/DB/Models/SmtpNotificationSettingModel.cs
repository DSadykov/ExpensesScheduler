using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace ExpensesScheduler.SmtpNotificationService.DB.Models;

[PrimaryKey(nameof(UserID))]
[Table("SmtpNotificationSettings")]
public class SmtpNotificationSettingModel
{
    public Guid UserID { get; set; }
    public bool IsActive {  get; set; }
    public required string Email { get; set; }
}
