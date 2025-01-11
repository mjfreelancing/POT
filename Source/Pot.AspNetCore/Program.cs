using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accounts.Extensions;
using Pot.AspNetCore.Features.Expenses.Extensions;

namespace Pot.AspNetCore;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder
            .AddCorrelationId()
            .AddLogging()
            .AddExceptionHandlers()
            .AddCustomProblemDetails()
            .AddValidation()
            .AddPotDbContext()
            .AddAccountServices()
            .AddExpenseServicess();

        var app = builder.Build();

        app.Logger.LogInformation("POT Startup: {AppStartup}", new { Local = DateTime.Now });

        app.UseCorrelationId()
           .UseExceptionHandler();

        // TODO: POT-14
        app.MapGet("/", () => "POT Summary");

        app.AddAccountEndpoints()
           .AddExpenseEndpoints();

        await app.RunAsync();
    }
}
