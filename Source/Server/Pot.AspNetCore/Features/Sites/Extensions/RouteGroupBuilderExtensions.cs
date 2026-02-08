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
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }
}
