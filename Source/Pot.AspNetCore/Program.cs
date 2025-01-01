using Microsoft.AspNetCore.Http.Features;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accounts.Import.Repository;
using Pot.AspNetCore.Features.Expenses.Import.Repository;
using Pot.Data;

namespace Pot.AspNetCore;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // builder.Logging.AddJsonConsole();

        builder.Services.AddDbContextFactory<PotDbContext>();

        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
        });


        builder.Services.AddScoped<IAccountImportRepository, AccountImportRepository>();
        builder.Services.AddScoped<IExpenseImportRepository, ExpenseImportRepository>();

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
