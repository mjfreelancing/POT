using Microsoft.AspNetCore.Http.Features;
using Pot.AspNetCore.Extensions;
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

        builder.AddLogging();

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

        // TODO: POT-14
        app.MapGet("/", () => "POT Summary");

        app.AddAccounts()
            .AddExpenses();

        await app.RunAsync();
    }
}
