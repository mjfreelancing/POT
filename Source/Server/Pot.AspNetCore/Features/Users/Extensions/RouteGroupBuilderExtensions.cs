using Pot.AspNetCore.Concerns.Auth;

namespace Pot.AspNetCore.Features.Users.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetAllUsers(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(UsersEndpoints.GetAll, GetAll.Handler.Invoke)
            .RequireAuthorization("user:view")
            .WithName(nameof(GetAllUsers))
            .WithSummary("Get all users")
            .WithDescription("Get all users")
            .Produces<GetAll.Response[]>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder UpdateUser(this RouteGroupBuilder routeGroupBuilder)
    {
        // Any signed-in user may edit their own details; editing anyone else needs user:manage. The route is
        // never anonymous, and a target outside the caller's site is refused as not-found.
        routeGroupBuilder
            .MapPut(UsersEndpoints.Update, Update.Handler.Invoke)
            .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser)
            .WithName(nameof(UpdateUser))
            .WithSummary("Update user details")
            .WithDescription("Updates existing user details")
            .Produces<Update.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder UpdateUserStatus(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut(UsersEndpoints.UpdateStatus, UpdateStatus.Handler.Invoke)
            .RequireAuthorization("user:manage")
            .WithName(nameof(UpdateUserStatus))
            .WithSummary("Update user status")
            .WithDescription("Updates user status")
            .Produces<UpdateStatus.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder UpdateUserRoles(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut(UsersEndpoints.UpdateRoles, UpdateRoles.Handler.Invoke)
            .RequireAuthorization("user:manage")
            .WithName(nameof(UpdateUserRoles))
            .WithSummary("Update user roles")
            .WithDescription("Updates user roles")
            .Produces<UpdateRoles.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder InviteUser(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(UsersEndpoints.Invite, Invite.Handler.Invoke)
            .RequireAuthorization("user:manage")
            .WithName(nameof(InviteUser))
            .WithSummary("Invite a new user")
            .WithDescription("Invite a new user to the same site as the caller")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder ResendInviteUser(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(UsersEndpoints.ResendInvite, ResendInvite.Handler.Invoke)
            .RequireAuthorization("user:manage")
            .WithName(nameof(ResendInviteUser))
            .WithSummary("Resends a user invitation")
            .WithDescription("Resends a user invitation")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }
}
