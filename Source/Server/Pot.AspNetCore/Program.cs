using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accounts.Extensions;
using Pot.AspNetCore.Features.Accruals.Extensions;
using Pot.AspNetCore.Features.Auth.Extensions;
using Pot.AspNetCore.Features.Expenses.Extensions;
using Pot.AspNetCore.Features.Incomes.Extensions;
using Pot.AspNetCore.Features.Maintenance.Extensions;
using Pot.AspNetCore.Features.Me.Extensions;
using Pot.AspNetCore.Features.Projections.Extensions;
using Pot.AspNetCore.Features.Roles.Extensions;
using Pot.AspNetCore.Features.Sites.Extensions;
using Pot.AspNetCore.Features.Users.Extensions;
using Pot.Data;

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
            .AddPotCors()
            .AddPotRateLimiting()
            .AddCorrelationId()
            .AddOpenApi()
            .AddHttpJsonOptions()
            .AddLogging()
            .AddCustomProblemDetails()
            .AddAspNetDependencies()
            .AddAspNetValidation()
            .AddSmtp()
            .AddPotData();

        var app = builder.Build();

        app.Logger.LogInformation("POT Startup: {AppStartup}", new { Local = DateTime.Now });

        app.UseExceptionHandler();
        app.MapHealthChecks("/_health");

        // UseCors must be called before UseAuthentication() and UseAuthorization() to ensure CORS headers are on all responses (including errors).
        // See AddPotCors() and CorsOptionsSetup for configuration setup - not using the overload with Action<CorsPolicyBuilder> since we need to load from configuration.
        app.UseCors();

        app.UseAuthentication()
           .UseAuthorization()
           .UseRateLimiter();

        app.UsePotMiddleware()
           .UseScalarOpenApi()
           .AddAuthEndpoints()
           .AddMeEndpoints()
           .AddUserEndpoints()
           .AddRoleEndpoints()
           .AddSiteEndpoints()
           .AddAccountEndpoints()
           .AddIncomeEndpoints()
           .AddExpenseEndpoints()
           .AddAccrualsEndpoints()
           .AddProjectionsEndpoints()
           .AddMaintenanceEndpoints();

        await app.RunAsync();
    }
}
