using Pot.AspNetCore.Middleware;

namespace Pot.AspNetCore.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseCorrelationId(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();

        return app;
    }
}