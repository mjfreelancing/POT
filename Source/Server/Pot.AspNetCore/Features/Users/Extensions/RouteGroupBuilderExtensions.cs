using System.Net;

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
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder UpdateUser(this RouteGroupBuilder routeGroupBuilder)
    {
        // There's no RequireAuthorization here since the user needs to be able to change their own display / email details
        routeGroupBuilder
            .MapPut(UsersEndpoints.Update, Update.Handler.Invoke)
            //.RequireAuthorization("user:manage")
            .WithName(nameof(UpdateUser))
            .WithSummary("Update user details")
            .WithDescription("Updates existing user details")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

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
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

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
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

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
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

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
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
