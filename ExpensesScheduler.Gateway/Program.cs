namespace ExpensesScheduler.Gateway;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        builder.Services.AddHealthChecks();

        // Add services to the container.

        var app = builder.Build();

        app.UseHealthChecks(new("/health"));

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        app.MapReverseProxy();

        app.Run();
    }
}