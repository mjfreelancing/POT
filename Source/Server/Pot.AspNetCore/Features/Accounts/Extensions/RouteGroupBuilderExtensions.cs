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
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
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
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

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
            .ProducesProblem(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

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
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

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
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return routeGroupBuilder;
    }
}
