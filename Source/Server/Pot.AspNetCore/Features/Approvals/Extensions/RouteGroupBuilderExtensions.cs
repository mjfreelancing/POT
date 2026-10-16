using Pot.AspNetCore.Concerns.Auth;

namespace Pot.AspNetCore.Features.Approvals.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetPendingApprovals(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(ApprovalsEndpoints.PendingApprovals, Pending.Handler.Invoke)
            .RequireAuthorization(AuthorizationPolicies.PlatformManage)
            .WithName(nameof(GetPendingApprovals))
            .WithSummary("Get all pending approvals")
            .WithDescription("Get all pending approvals")
            .Produces<Pending.Response[]>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder UpdateUserApproval(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut(ApprovalsEndpoints.UpdateStatus, UpdateStatus.Handler.Invoke)
            .RequireAuthorization(AuthorizationPolicies.PlatformManage)
            .WithName(nameof(UpdateUserApproval))
            .WithSummary("Update a user pending approval")
            .WithDescription("Update a user pending approval")
            .Produces<UpdateStatus.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }
}
