using Pot.AspNetCore.Endpoints.Handlers;

namespace Pot.AspNetCore.Extensions;

internal static class RouteBuilderExtensions
{
    public static RouteGroupBuilder AddAccountsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("", Accounts.GetAll);

        return group;
    }

    public static RouteGroupBuilder AddExpensesEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("", Expenses.GetAll);

        return group;
    }
}
