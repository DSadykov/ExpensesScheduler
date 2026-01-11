using ExpensesScheduler.Authorization.Middleware;
using ExpensesScheduler.Authorization.Models;
using ExpensesScheduler.ExpensesSchedulerService.DB;
using ExpensesScheduler.ExpensesSchedulerService.DTO.Responses;
using ExpensesScheduler.ExpensesSchedulerService.Services;
using ExpensesScheduler.ExpensesSchedulerService.Services.Background;
using ExpensesScheduler.Messaging;
using ExpensesScheduler.Messaging.DTO;

using Hangfire;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

SetupServices(builder.Services, builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard("/hangfire");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapSwagger();


app.UseHealthChecks(new("/health"));

app.UseMiddleware<JwtMiddleware>();

app.Run();

void SetupServices(IServiceCollection services, ConfigurationManager configuration)
{
    var connectionString = configuration.GetConnectionString("default");
    var hangFireConnectionString = configuration.GetConnectionString("hangfire");
    services.AddHealthChecks();
    services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.SelectMany(m => m.Value.Errors)
                                           .Select(e => e.ErrorMessage)
                                           .ToList();


            var customErrorResponse = new BaseResponse()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                ErrorMessage = string.Join(';',errors),
            };

            return new BadRequestObjectResult(customErrorResponse);
        };
    });

    services.AddStackExchangeRedisCache(x =>
    {
        x.Configuration = configuration.GetConnectionString("redis");
        x.InstanceName = "local";
    });
    services.AddLogging(x =>
    {
        x.AddConsole();
        x.AddDebug();
        x.SetMinimumLevel(LogLevel.Debug);
    });
    services.AddSwaggerGen();
    services.AddHangfire(x =>
    {
        x.UseSqlServerStorage(hangFireConnectionString);
    });
    services.Configure<AuthorizationSettings>(configuration.GetSection("Authorization"));
    services.AddHangfireServer();
    services.AddProducer<NotificationMessage>(configuration, "Kafka:Notification");
    services.AddDbContextFactory<ScheduledExpensesDbContext>(o =>
    {
        o.UseSqlServer(connectionString);
    });
    services.AddSingleton<INotificationMessageSender, NotificationMessageSender>();
    services.AddTransient<JwtMiddleware>();

    // Register concrete service and caching decorator
    services.AddScoped<ISheduledExpensesService, SheduledExpensesService>();
    services.Decorate<ISheduledExpensesService, CachingSheduledExpensesServiceDecorator>();

    services.AddHostedService<ScheduledExpensesWorker>();
}