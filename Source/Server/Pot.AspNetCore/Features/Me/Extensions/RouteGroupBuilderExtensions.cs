using Pot.AspNetCore.Concerns.Auth;

namespace Pot.AspNetCore.Features.Me.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetMe(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(MeEndpoints.Me, Get.Handler.Invoke)
            .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser)
            .WithName(nameof(GetMe))
            .WithSummary("Get User Info")
            .WithDescription("Get the current user's information and permissions")
            .Produces<Get.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder ChangePassword(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut(MeEndpoints.ChangePassword, Me.ChangePassword.Handler.Invoke)
            .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser)
            .WithName(nameof(ChangePassword))
            .WithSummary("Change Password")
            .WithDescription("Change the user password")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }
}
