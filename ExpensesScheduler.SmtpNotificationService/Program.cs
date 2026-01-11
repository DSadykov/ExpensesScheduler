

using ExpensesScheduler.Messaging;
using ExpensesScheduler.Messaging.DTO;
using ExpensesScheduler.SmtpNotificationService.DB;
using ExpensesScheduler.SmtpNotificationService.MessageHandlers;
using ExpensesScheduler.SmtpNotificationService.Services;
using ExpensesScheduler.SmtpNotificationService.Services.Models;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

SetupServices(builder.Services, builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseHealthChecks(new("/health"));

app.MapControllers();

app.Run();

void SetupServices(IServiceCollection services, ConfigurationManager configuration)
{
    services.AddControllers();

    var connectionString = configuration.GetConnectionString("default");

    services.AddHealthChecks();

    services.AddDbContextFactory<SmtpNotificationSettingsDbContext>(o =>
    {
        o.UseSqlServer(connectionString);
    });

    services.AddConsumer<SmtpNotificationMessage, SmtpNotificationHandler>
        (configuration, "Kafka:SmtpNotification");
    services.AddConsumer<SmtpNewUserCreatedMessage, SmtpNewUserCreatedHandler>
        (configuration, "Kafka:SmtpNewUser");

    services.Configure<SmtpSettings>(configuration.GetSection("SmtpClient"));

    services.AddSingleton<INotificationMessageSender<SmtpMessageModel>, SmtpMessageSender>();
}