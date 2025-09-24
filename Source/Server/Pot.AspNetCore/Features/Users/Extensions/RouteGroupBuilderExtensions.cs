using Pot.AspNetCore.Features.Users.Me;
using System.Net;

namespace Pot.AspNetCore.Features.Users.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetMe(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(UsersEndpoints.Me, Handler.Invoke)
            .RequireAuthorization("AuthenticatedUser")
            .WithName(nameof(GetMe))
            .WithSummary("Get User Info")
            .WithDescription("Get the current user's information and permissions")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.Unauthorized)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
