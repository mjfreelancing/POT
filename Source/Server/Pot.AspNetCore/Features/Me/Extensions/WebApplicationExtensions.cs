using Pot.AspNetCore.Concerns.RateLimiting;
using Pot.AspNetCore.Extensions;

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
                .ProducesStandardProblems()
                .RequireRateLimiting(RateLimiterPolicy.Chained)
                .GetMe()
                .ChangePassword();
        }

        return app;
    }
}
