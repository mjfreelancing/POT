using System.Net;

namespace Pot.AspNetCore.Features.Roles.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetAllRoles(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(RolesEndpoints.GetAll, GetAll.Handler.Invoke)
            .RequireAuthorization("user:view")
            .WithName(nameof(GetAllRoles))
            .WithSummary("Get all roles")
            .WithDescription("Get all roles")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
