using ExpensesScheduler.AuthorizationService.DB.Models;

using Microsoft.EntityFrameworkCore;

namespace ExpensesScheduler.AuthorizationService.DB;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<UserModel> UserModels { get; set; }
    public async Task<UserModel> AddUserAsync(string password, string email)
    {
        var createdUserEntity = await UserModels.AddAsync(new()
        {
            Email   = email,
            Password = password,
            ID = Guid.NewGuid(),
            IsEmailConfirmed = false
        });
        await SaveChangesAsync();

        return createdUserEntity.Entity;
    }
    public async Task<UserModel?> GetUserAsync(string? email = null)
    {
        if(email == null)
        {
            return null;
        }

        return UserModels.FirstOrDefault(x =>
        (x.Email == email));
    }
    
    public bool CheckUserExists(string email)
    {
        return UserModels.Any(x=>x.Email == email);
    }
}
