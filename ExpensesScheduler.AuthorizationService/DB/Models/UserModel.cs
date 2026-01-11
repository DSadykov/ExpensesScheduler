using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

namespace ExpensesScheduler.AuthorizationService.DB.Models;

[Table("Users")]
[PrimaryKey(nameof(ID))]
[Index(nameof(Email), IsUnique = true)]
public class UserModel
{
    [Key]
    public Guid ID { get; set; }
    [EmailAddress]
    public required string Email { get; set; }
    [PasswordPropertyText]
    [JsonIgnore]
    public required string Password { get; set; }
    [JsonIgnore]
    public bool IsEmailConfirmed { get; set; }
}
