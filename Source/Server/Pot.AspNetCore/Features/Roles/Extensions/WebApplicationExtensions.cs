using Pot.AspNetCore.Concerns.RateLimiting;

namespace Pot.AspNetCore.Features.Roles.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddRoleEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Role Routes]"))
        {
            app.Logger.LogInformation("Adding role endpoints");

            app.MapGroup(RolesEndpoints.Group)
                .RequireRateLimiting(RateLimiterPolicy.Chained)
                .WithTags(RolesEndpoints.Tag)
                .GetAllRoles();
        }

        return app;
    }
}
