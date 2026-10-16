using Pot.AspNetCore.Concerns.RateLimiting;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Features.Users.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddUserEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup User Routes]"))
        {
            app.Logger.LogInformation("Adding user endpoints");

            var group = app
                .MapGroup(UsersEndpoints.Group)
                .WithTags(UsersEndpoints.Tag)
                .ProducesStandardProblems()
                .RequireRateLimiting(RateLimiterPolicy.Chained)
                .GetAllUsers()
                .UpdateUser()
                .UpdateUserStatus()
                .UpdateUserRoles()
                .InviteUser()
                .ResendInviteUser();
        }

        return app;
    }
}
