using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accounts.Extensions;
using Pot.AspNetCore.Features.Expenses.Extensions;
using Pot.AspNetCore.Features.Expenses.Import.Validators;

namespace Pot.AspNetCore;

public class Program
{
    public static async Task Main(string[] args)
    {
        // ASPNETCORE_ENVIRONMENT => Environment name

        var builder = WebApplication.CreateBuilder(args);

        builder
            .AddCorrelationId()
            .AddOpenApi()
            .AddLogging()
            .AddExceptionHandlers()
            .AddCustomProblemDetails()
            .AutoRegisterPotDependencies()
            .AddValidation()
            .AddPotData();


        // https://www.youtube.com/watch?v=3XoXzEPHdTA Nick Chapsas
        // https://www.youtube.com/watch?v=6DWJIyipxzw Milan Jovanovic

        //builder.Services.AddAuthorization();

        //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //    .AddJwtBearer(options =>
        //    {
        //        options.TokenValidationParameters = new TokenValidationParameters
        //        {
        //            ValidateIssuer = true,
        //            ValidateAudience = true,
        //            ValidateLifetime = true,
        //            ValidateIssuerSigningKey = true,
        //            ValidIssuer = "https://pot.mjfreelancing.com",
        //            ValidAudience = "https://pot.mjfreelancing.com",
        //            IssuerSigningKey = new SymmetricSecurityKey("some_secret_key"u8.ToArray())
        //        };
        //    });


        // TODO: Move somewhere else
        builder.Services.AddScoped<IExpenseImportValidator, ExpenseImportValidator>();


        var app = builder.Build();

        app.Logger.LogInformation("POT Startup: {AppStartup}", new { Local = DateTime.Now });



        //app.UseAuthentication();
        //app.UseAuthorization();



        app.UseCorrelationId()
           .UseScalarOpenApi()
           .UseExceptionHandler();

        // TODO: POT-14
        app.MapGet("/", () => "POT Summary");

        // 200 - Success
        // 304 - Not Modified
        // 422 - Validation and other errors that occur due to data related problems
        //       (such as conflicts, constraints, etc) when processing the input data
        // 500 - Unexpected errors
        app.AddAccountEndpoints()
           .AddExpenseEndpoints();

        await app.RunAsync();
    }
}
