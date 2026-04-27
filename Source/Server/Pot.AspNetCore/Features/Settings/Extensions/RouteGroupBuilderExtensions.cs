using Pot.AspNetCore.Features.Settings.Upsert;

namespace Pot.AspNetCore.Features.Settings.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetAllSettings(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(SettingsEndpoints.GetAll, GetAll.Handler.Invoke)
            .RequireAuthorization("site:view")
            .WithName(nameof(GetAllSettings))
            .WithSummary("Get all site settings")
            .WithDescription("Retrieve all settings for the current site as a flat list")
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder UpdateSettings(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut(SettingsEndpoints.Update, Handler.Invoke)
            .RequireAuthorization("site:manage")
            .WithName(nameof(UpdateSettings))
            .WithSummary("Update a setting")
            .WithDescription("Create or update a single setting for the current site")
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }
}
