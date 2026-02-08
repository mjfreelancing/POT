namespace Pot.AspNetCore.Features.Projections.Extensions;

internal static class RouteGroupBuilderExtensions
{
    // TODO: Update ProducesProblem() across all routes

    public static RouteGroupBuilder GetProjections(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(ProjectionsEndpoints.Get, Get.Handler.Invoke)
            .RequireAuthorization("account:view")
            .WithName(nameof(GetProjections))
            .WithSummary("Get projections")
            .WithDescription("Get projected financial position")
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }
}
