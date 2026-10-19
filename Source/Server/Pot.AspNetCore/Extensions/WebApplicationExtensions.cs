using Pot.AspNetCore.Concerns.Middleware;
using Scalar.AspNetCore;

namespace Pot.AspNetCore.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication UseCorrelationId(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();

        return app;
    }

    public static WebApplication UsePotMiddleware(this WebApplication app)
    {
        if (!app.Environment.IsProduction())
        {
            app.UseMiddleware<RawRequestLoggingMiddleware>();
        }

        app.UseMiddleware<UserContextMiddleware>();

        return app;
    }

    public static WebApplication UseScalarOpenApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // To require authorization, see : https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi
            // Both are anonymous: the fail-closed fallback policy would otherwise demand a bearer token that the
            // browser cannot present, and the documentation page would stop loading. This branch is Development only.
            app.MapOpenApi().AllowAnonymous();

            app.MapScalarApiReference().AllowAnonymous();
        }

        return app;
    }
}