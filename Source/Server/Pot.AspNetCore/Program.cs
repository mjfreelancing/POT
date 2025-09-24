using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accounts.Extensions;
using Pot.AspNetCore.Features.Auth.Extensions;
using Pot.AspNetCore.Features.Expenses.Extensions;
using Pot.AspNetCore.Features.Incomes.Extensions;
using Pot.AspNetCore.Features.Maintenance.Extensions;
using Pot.AspNetCore.Features.Projections.Extensions;
using Pot.Data;
using Pot.Data.Extensions;

namespace Pot.AspNetCore;

public class Program
{
    public static async Task Main(string[] args)
    {
        // ASPNETCORE_ENVIRONMENT => Environment name

        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddHealthChecks()
            .AddDbContextCheck<PotDbContext>();

        builder.Services.AddCors();

        builder
            .AddExceptionHandlers()
            .AddPotAuth()
            .AddCorrelationId()
            .AddOpenApi()
            .AddHttpJsonOptions()
            .AddLogging()
            .AddCustomProblemDetails()
            .AddAspNetDependencies()
            .AddAspNetValidation()
            .AddPotData(builder.Configuration.GetConnectionString());

        var app = builder.Build();

        app.Logger.LogInformation("POT Startup: {AppStartup}", new { Local = DateTime.Now });

        app.UseExceptionHandler();
        app.MapHealthChecks("/_health");

        // UseCors must be called before UseAuthentication() and UseAuthorization() to ensure CORS headers are on all responses (including errors)
        app.UseCors(policy => policy
            .WithOrigins("http://localhost:5175" /*, "http://localhost:4173"*/ ) // Allow frontend URL
            .AllowAnyMethod()
            .AllowAnyHeader()

            // Required when using authentication (note, WithOrigins() cannot use * with AllowCredentials)
            .AllowCredentials()

            // Exposing 'content-disposition' allows the client to handle the file download correctly (get the filename)
            .WithExposedHeaders("content-disposition"));

        app.UseAuthentication();
        app.UseAuthorization();

        app.UsePotMiddleware()
           .UseScalarOpenApi();

        // 200 - Success
        // 304 - Not Modified
        // 401 - Unauthorized
        // 422 - Validation and other errors that occur due to data related problems
        //       (such as conflicts, constraints, etc) when processing the input data
        // 500 - Unexpected errors
        app.AddAuthEndpoints()
           .AddAccountEndpoints()
           .AddIncomeEndpoints()
           .AddExpenseEndpoints()
           .AddProjectionsEndpoints()
           .AddMaintenanceEndpoints();

        await app.RunAsync();
    }
}
