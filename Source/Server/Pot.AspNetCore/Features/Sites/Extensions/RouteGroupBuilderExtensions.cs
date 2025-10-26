using System.Net;

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
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
