
using ExpensesScheduler.Authorization.Models;
using ExpensesScheduler.AuthorizationService.DB;
using ExpensesScheduler.AuthorizationService.Services;
using ExpensesScheduler.Messaging;
using ExpensesScheduler.Messaging.DTO;

using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        SetupServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHealthChecks(new("/health"));

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    private static void SetupServices(IServiceCollection services, ConfigurationManager configuration)
    {

        // Add services to the container.
        var connectionString = configuration.GetConnectionString("default");

        services.AddSwaggerGen();
        services.AddControllers();
        services.AddHealthChecks();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();
        services.Configure<AuthorizationSettings>(configuration.GetSection("Authorization"));
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddProducer<NewUserCreatedMessage>(configuration, "Kafka:NewUser");
        services.AddDbContext<UsersDbContext>(o =>
        {
            o.UseSqlServer(connectionString);
        });
    }
}