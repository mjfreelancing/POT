using System.Net;

namespace Pot.AspNetCore.Features.Accruals.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder AccrueAccountExpenses(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AccrualsEndpoints.AccrueExpenses, AccrueExpenses.Handler.Invoke)
            .RequireAuthorization("account:manage")
            .WithName(nameof(AccrueExpenses))
            .WithSummary("Accrue expenses")
            .WithDescription("Accrue expenses associated with the account")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
