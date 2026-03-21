using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accounts.Extensions;
using Pot.AspNetCore.Features.Accruals.Extensions;
using Pot.AspNetCore.Features.Approvals.Extensions;
using Pot.AspNetCore.Features.Auth.Extensions;
using Pot.AspNetCore.Features.Expenses.Extensions;
using Pot.AspNetCore.Features.Incomes.Extensions;
using Pot.AspNetCore.Features.Maintenance.Extensions;
using Pot.AspNetCore.Features.Me.Extensions;
using Pot.AspNetCore.Features.Projections.Extensions;
using Pot.AspNetCore.Features.Roles.Extensions;
using Pot.AspNetCore.Features.Settings.Extensions;
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

        // Not required since we are behind a reverse proxy in Azure that handles HTTPS
        // app.UseHttpsRedirection();

        app.MapHealthChecks("/_health");

        // REQUIRED ORDER:
        // 1. UseCors() must run before authentication/authorization so CORS headers are included on all responses (including failures).
        // 2. UseAuthentication() must run before UsePotMiddleware() because UserContextMiddleware reads HttpContext.User
        //    and populates ICurrentUserContext, which downstream data/repository code depends on.
        // 3. Do not move UsePotMiddleware() ahead of UseAuthentication() unless UserContextMiddleware is redesigned.
        // See AddPotCors() and CorsOptionsSetup for configuration setup - not using the overload with Action<CorsPolicyBuilder> since we need to load from configuration.
        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        app.UsePotMiddleware();

        app.UseScalarOpenApi()
           .AddAuthEndpoints()
           .AddApprovalEndpoints()
           .AddMeEndpoints()
           .AddUserEndpoints()
           .AddRoleEndpoints()
           .AddSiteEndpoints()
           .AddAccountEndpoints()
           .AddIncomeEndpoints()
           .AddExpenseEndpoints()
           .AddAccrualsEndpoints()
           .AddProjectionsEndpoints()
           .AddMaintenanceEndpoints()
           .AddSettingsEndpoints();

        await app.RunAsync();
    }
}
