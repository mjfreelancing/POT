using System.Net;

namespace Pot.AspNetCore.Features.Auth.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder LoginUser(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AuthEndpoints.Login, Login.Handler.Invoke)
            .WithName(nameof(LoginUser))
            .WithSummary("Login")
            .WithDescription("Login the user")
            .WithTags("Auth")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder RefreshToken(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AuthEndpoints.Refresh, Refresh.Handler.Invoke)
            .WithName(nameof(RefreshToken))
            .WithSummary("Refresh Token")
            .WithDescription("Refresh the user access token")
            .WithTags("Auth")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder GetMe(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(AuthEndpoints.Me, Me.Handler.Invoke)
            .RequireAuthorization("AuthenticatedUser")
            .WithName(nameof(GetMe))
            .WithSummary("Get User Info")
            .WithDescription("Get the current user's information and permissions")
            .WithTags("Auth")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.Unauthorized)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder ChangePassword(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut(AuthEndpoints.ChangePassword, Auth.ChangePassword.Handler.Invoke)
            .RequireAuthorization("AuthenticatedUser")
            .WithName(nameof(ChangePassword))
            .WithSummary("Change Password")
            .WithDescription("Change the user password")
            .WithTags("Auth")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
