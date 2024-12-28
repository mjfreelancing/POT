using Pot.Data.Dtos;

namespace Pot.AspNetCore.Extensions;

internal static class RouteBuilderExtensions
{
    public static RouteGroupBuilder AddAccountsEndpoints(this RouteGroupBuilder group)
    {
        group
            .MapGet("", Endpoints.Accounts.GetAll.Handler.GetAllAccounts)
            .Produces<List<AccountDto>>();

        return group;
    }

    public static RouteGroupBuilder AddExpensesEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("", Endpoints.Expenses.GetAll.Handler.GetAllExpenses);

        return group;
    }
}
