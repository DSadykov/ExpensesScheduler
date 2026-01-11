
using ExpensesScheduler.SmtpNotificationService.DB.Models;

using Microsoft.EntityFrameworkCore;

namespace ExpensesScheduler.SmtpNotificationService.DB;

public class SmtpNotificationSettingsDbContext : DbContext
{
    public SmtpNotificationSettingsDbContext(DbContextOptions options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<SmtpNotificationSettingModel> NotificationSettings { get; set; }

    public async Task AddNewUserAsync(Guid userID, string email)
    {
        await NotificationSettings.AddAsync(new() { Email = email, UserID = userID , IsActive = true});
    
        await SaveChangesAsync();
    }
}