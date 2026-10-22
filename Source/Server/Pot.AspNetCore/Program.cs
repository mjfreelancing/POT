using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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

namespace Pot.AspNetCore;

/// <summary>Provides the API application entry point.</summary>
public class Program
{
    /// <summary>Builds and runs the API application.</summary>
    /// <param name="args">The command-line arguments passed to the application.</param>
    public static async Task Main(string[] args)
    {
        // ASPNETCORE_ENVIRONMENT => Environment name

        var builder = WebApplication.CreateBuilder(args);

        builder
            .AddPotHealthChecks()
            .AddExceptionHandlers()
            .AddPotAuth()
            .AddPotCors()
            .AddPotRateLimiting()
            .AddPotMiddleware()
            .AddOpenApi()
            .AddHttpJsonOptions()
            .AddLogging()
            .AddProblemDetails()
            .AddAspNetDependencies()
            .AddAspNetValidation()
            .AddSmtp()
            .AddPotData();

        var app = builder.Build();

        app.Logger.LogInformation("POT Startup: {AppStartup}", new { Local = DateTime.Now });

        // First, so every response carries X-Content-Type-Options and Cache-Control - including the 500s and status code
        // pages written by the two calls below, and the 401, 403 and 429 responses refused further down.
        app.UseSecurityHeaders();

        // One entry per request on the fields chosen in AddLogging, written when the response completes - after the
        // exception handler and status code pages below have decided the status the caller receives. The health probes
        // are short-circuited at routing and never reach it; no request body or header is logged in any environment.
        app.UseHttpLogging();

        app.UseExceptionHandler();

        // Write ProblemDetails for error responses that otherwise have no body.
        app.UseStatusCodePages();

        // Not required since we are behind a reverse proxy in Azure that handles HTTPS
        // app.UseHttpsRedirection();

        // Liveness: always Healthy while the app is running — no checks are executed.
        // Used by production monitoring (called every 10 s) so no DB query is incurred.
        // Anonymous: polled by infrastructure that holds no credentials.
        //
        // ShortCircuit() ends the request at routing, so a probe reaches none of the middleware above: no request-log
        // entry, no security headers and no correlation id. Its caller reads the status, and there is nothing user-facing
        // or cacheable in the plain-text response. AllowAnonymous() stays as belt and braces, because routing skips
        // authorization for a short-circuited endpoint.
        app.MapHealthChecks("/_health", new HealthCheckOptions
        {
            Predicate = _ => false,
        })
        .AllowAnonymous()
        .ShortCircuit();

        // Readiness: runs checks tagged "ready" to confirm the schema is fully migrated.
        // Used by E2E tests to gate on DB and schema availability.
        // Background workers gate using IHealthCheckService directly (by name), not via this endpoint.
        // Anonymous: polled by infrastructure and by the E2E gate before a session can exist.
        app.MapHealthChecks("/_health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
        })
        .AllowAnonymous()
        .ShortCircuit();

        // REQUIRED ORDER:
        // 1. UseCors() must run before authentication/authorization so CORS headers are included on all responses (including failures).
        // 2. UseCorrelationId() must run straight after UseCors(): after it, so its own 400 rejection carries CORS headers, and
        //    before authentication, authorization and rate limiting, so their 401/403/429 responses and log entries carry the
        //    client's correlation id rather than the server-generated one.
        // 3. UseAuthentication() must run before UsePotMiddleware() because UserContextMiddleware reads HttpContext.User
        //    and populates ICurrentUserContext, which downstream data/repository code depends on.
        // 4. Do not move UsePotMiddleware() ahead of UseAuthentication() unless UserContextMiddleware is redesigned.
        // See AddPotCors() and CorsOptionsSetup for configuration setup - not using the overload with Action<CorsPolicyBuilder> since we need to load from configuration.
        app.UseCors();

        app.UseCorrelationId();

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
