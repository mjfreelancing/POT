
namespace Pot.AspNetCore.Features.Sites.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder UpdateSite(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut(SitesEndpoints.Update, Update.Handler.Invoke)
            .RequireAuthorization("site:manage")
            .WithName(nameof(UpdateSite))
            .WithSummary("Update site details")
            .WithDescription("Updates existing site details")
            .Produces<Update.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder DeleteSite(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapDelete(SitesEndpoints.Delete, Delete.Handler.Invoke)
            .RequireAuthorization("site:manage")
            .WithName(nameof(DeleteSite))
            .WithSummary("Delete the current site")
            .WithDescription("Deletes the caller's site")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return routeGroupBuilder;
    }
}
