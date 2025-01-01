using Microsoft.AspNetCore.Http.Features;
using Pot.AspNetCore.Features.Accounts.Extensions;
using Pot.AspNetCore.Features.Expenses.Extensions;
using Pot.AspNetCore.Features.Expenses.GetAll.Extensions;
using Pot.Data.Extensions;

namespace Pot.AspNetCore;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Alternative to the default SimpleConsole Logger
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
        //
        //builder.Logging.AddJsonConsole(options =>
        //{
        //    options.IncludeScopes = false;
        //    options.TimestampFormat = "HH:mm:ss";
        //    options.JsonWriterOptions = new JsonWriterOptions
        //    {
        //        Indented = true
        //    };
        //});

        builder.Logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
            options.UseUtcTimestamp = true;
        });

        builder.Services.Configure<FormOptions>(options =>
        {
            // Limit the size of files that can be imported to 10MB
            options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
        });

        builder.Services
            .AddPotDbContext()
            .AddAccounts()
            .AddExpenses();

        var app = builder.Build();

        app.Logger.LogInformation("POT Startup: {AppStartup}", new { Local = DateTime.Now });

        app.MapGet("/", () => "POT");

        app
            .AddAccounts()
            .AddExpenses();

        await app.RunAsync();
    }
}
