
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
            .Produces<GetAll.Response[]>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return routeGroupBuilder;
    }
}
