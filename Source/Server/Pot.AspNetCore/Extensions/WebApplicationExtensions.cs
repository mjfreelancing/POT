using Pot.AspNetCore.Concerns.Middleware;
using Scalar.AspNetCore;

namespace Pot.AspNetCore.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication UsePotMiddleware(this WebApplication app)
    {
        if (!app.Environment.IsProduction())
        {
            app.UseMiddleware<RawRequestLoggingMiddleware>();
        }

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<UserContextMiddleware>();

        return app;
    }

    public static WebApplication UseScalarOpenApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // To require authorization, see : https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi
            app.MapOpenApi();

            app.MapScalarApiReference();
        }

        return app;
    }
}