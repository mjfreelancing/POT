using Pot.AspNetCore.Extensions;
using Pot.Data;

namespace Pot.AspNetCore;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // builder.Logging.AddJsonConsole();

        builder.Services.AddDbContextFactory<PotDbContext>();

        var app = builder.Build();

        app.Logger.LogInformation("POT has started");

        app.MapGet("/", () => "POT");

        app.MapGroup("api/accounts")
            .AddAccountsEndpoints()
            .WithTags("Accounts Api");

        app.MapGroup("api/expenses")
            .AddExpensesEndpoints()
            .WithTags("Expenses Api");

        app.Run();
    }
}
