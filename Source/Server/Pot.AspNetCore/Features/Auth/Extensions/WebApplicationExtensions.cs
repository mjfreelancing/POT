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
                .LoginUser()
                .LogoutUser()
                .RefreshToken()
                .PasswordResetSend()
                .PasswordResetVerify();
        }

        return app;
    }
}
