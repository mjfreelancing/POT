using Pot.AspNetCore.Concerns.RateLimiting;

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
