using System.Net;

namespace Pot.AspNetCore.Features.Users.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder UpdateUser(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut(UsersEndpoints.Update, Update.Handler.Invoke)
            .RequireAuthorization("user:manage")
            .WithName(nameof(UpdateUser))
            .WithSummary("Update user details")
            .WithDescription("Updates existing user details")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
