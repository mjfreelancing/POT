using System.Net;

namespace Pot.AspNetCore.Features.Accounts.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetAllAccounts(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(AccountsEndpoints.GetAll, GetAll.Handler.Invoke)
            .RequireAuthorization("account:view")
            .WithName(nameof(GetAllAccounts))
            .WithSummary("Get all accounts")
            .WithDescription("Get all account details")
            .WithTags("Accounts")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder GetAccount(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(AccountsEndpoints.Get, Get.Handler.Invoke)
            .RequireAuthorization("account:view")
            .WithName(nameof(GetAccount))
            .WithSummary("Get account")
            .WithDescription("Get details for an existing account")
            .WithTags("Accounts")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder CreateAccount(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AccountsEndpoints.Create, Create.Handler.Invoke)
            .RequireAuthorization("account:manage")
            .WithName(nameof(CreateAccount))
            .WithSummary("Create account")
            .WithDescription("Create new account details")
            .WithTags("Accounts")
            .ProducesProblem((int)HttpStatusCode.Created)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder UpdateAccount(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut(AccountsEndpoints.Update, Update.Handler.Invoke)
            .RequireAuthorization("account:manage")
            .WithName(nameof(UpdateAccount))
            .WithSummary("Update account")
            .WithDescription("Updates existing account details")
            .WithTags("Accounts")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder DeleteAccount(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapDelete(AccountsEndpoints.Delete, Delete.Handler.Invoke)
            .RequireAuthorization("account:manage")
            .WithName(nameof(DeleteAccount))
            .WithSummary("Delete account")
            .WithDescription("Deletes existing account details")
            .WithTags("Accounts")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder AccrueAccountExpenses(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(AccountsEndpoints.AccrueExpenses, AccrueExpenses.Handler.Invoke)
            .RequireAuthorization("account:manage")
            .WithName(nameof(AccrueExpenses))
            .WithSummary("Accrue expenses")
            .WithDescription("Accrue expenses associated with the account")
            .WithTags("Accounts")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
