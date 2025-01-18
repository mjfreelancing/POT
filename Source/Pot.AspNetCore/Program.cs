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
            .AddOpenApi()
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
           .UseScalarOpenApi()
           .UseExceptionHandler();

        // TODO: POT-14
        app.MapGet("/", () => "POT Summary");

        // 422 - Validation and other errors that occur due to data related problems
        //       (such as conflicts, constraints, etc) when processing the input data
        // 500 - Unexpected errors
        app.AddAccountEndpoints()
           .AddExpenseEndpoints();

        await app.RunAsync();
    }
}
