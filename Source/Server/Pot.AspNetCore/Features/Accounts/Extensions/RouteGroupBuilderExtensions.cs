using Pot.AspNetCore.Extensions;

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
            .Produces<GetAll.Response[]>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

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
            .Produces<Get.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

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
            .Produces<Create.Response>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesRequestBodyProblems();

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
            .Produces<Update.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesStaleEntityTagConflict()
            .ProducesRequestBodyProblems();

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
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }
}
