using System.Net;

namespace Pot.AspNetCore.Features.Me.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetMe(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(MeEndpoints.Me, Get.Handler.Invoke)
            .RequireAuthorization("AuthenticatedUser")
            .WithName(nameof(GetMe))
            .WithSummary("Get User Info")
            .WithDescription("Get the current user's information and permissions")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.Unauthorized)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder ChangePassword(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut(MeEndpoints.ChangePassword, Me.ChangePassword.Handler.Invoke)
            .RequireAuthorization("AuthenticatedUser")
            .WithName(nameof(ChangePassword))
            .WithSummary("Change Password")
            .WithDescription("Change the user password")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
