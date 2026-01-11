

using ExpensesScheduler.Messaging;
using ExpensesScheduler.Messaging.DTO;
using ExpensesScheduler.NotificationService.MessageHandlers;
using ExpensesScheduler.NotificationService.Services.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

SetupServices(builder.Services, builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}
app.UseHealthChecks(new("/health"));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void SetupServices(IServiceCollection services, ConfigurationManager configuration)
{

    services.AddControllers();

    services.AddHealthChecks();

    services.AddConsumer<NotificationMessage, NotificationHandler>(configuration, "Kafka:Notification");

    services.AddProducer<SmtpNotificationMessage>(configuration, "Kafka:SmtpNotification");

    services.AddConsumer<NewUserCreatedMessage, NewUserCreatedHandler>(configuration, "Kafka:NewUser");

    services.AddProducer<SmtpNewUserCreatedMessage>(configuration, "Kafka:SmtpNewUser");

    services.Configure<NotificationSettings>(configuration.GetSection("NotificationSettings"));
}