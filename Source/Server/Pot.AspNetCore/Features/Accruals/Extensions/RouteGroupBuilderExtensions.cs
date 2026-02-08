namespace Pot.AspNetCore.Features.Accruals.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetStatus(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(AccrualsEndpoints.Status, Status.Handler.Invoke)
            .RequireAuthorization("account:view", "expense:view", "income:view")
            .WithName(nameof(GetStatus))
            .WithSummary("Accruals status")
            .WithDescription("Get accruals status for all accounts")
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder AccrueAccountExpenses(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AccrualsEndpoints.AccrueExpenses, AccrueExpenses.Handler.Invoke)
            .RequireAuthorization("account:manage")
            .WithName(nameof(AccrueAccountExpenses))
            .WithSummary("Accrue expenses")
            .WithDescription("Accrue expenses associated with one or more accounts")
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }
}
