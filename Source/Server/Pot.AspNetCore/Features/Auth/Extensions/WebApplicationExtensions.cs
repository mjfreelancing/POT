using Pot.AspNetCore.Concerns.RateLimiting;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Features.Auth.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddAuthEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Auth Routes]"))
        {
            app.Logger.LogInformation("Adding auth endpoints");

            var group = app
                .MapGroup(AuthEndpoints.Group)
                .WithTags(AuthEndpoints.Tag)
                .ProducesStandardProblems()
                .RequireRateLimiting(RateLimiterPolicy.Chained)
                .LoginUser()
                .LogoutUser()
                .RefreshToken()
                .PasswordResetSend()
                .PasswordResetVerify()
                .SignupSend()
                .SignupComplete();
        }

        return app;
    }
}
