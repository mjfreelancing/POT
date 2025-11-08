using Pot.AspNetCore.Concerns.RateLimiting;

namespace Pot.AspNetCore.Features.Me.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddMeEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Me Routes]"))
        {
            app.Logger.LogInformation("Adding 'me' endpoints");

            var group = app
                .MapGroup(MeEndpoints.Group)
                .WithTags(MeEndpoints.Tag)
                .RequireRateLimiting(RateLimiterPolicy.Chained)
                .GetMe()
                .ChangePassword();
        }

        return app;
    }
}
