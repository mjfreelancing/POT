using Pot.AspNetCore.Features.Expenses.ToggleExclude;

namespace Pot.AspNetCore.Features.Expenses.Extensions;

internal static class RouteGroupBuilderExtensions
{
    // TODO: Update ProducesProblem() across all routes

    public static RouteGroupBuilder GetAllExpenses(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(ExpensesEndpoints.GetAll, GetAll.Handler.Invoke)
            .RequireAuthorization("expense:view")
            .WithName(nameof(GetAllExpenses))
            .WithSummary("Get all expenses")
            .WithDescription("Get all expense details")
            .Produces<GetAll.Response[]>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder GetExpense(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(ExpensesEndpoints.Get, Get.Handler.Invoke)
            .RequireAuthorization("expense:view")
            .WithName(nameof(GetExpense))
            .WithSummary("Get expense")
            .WithDescription("Get details for an existing expense")
            .Produces<Get.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder CreateExpense(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(ExpensesEndpoints.Create, Create.Handler.Invoke)
            .RequireAuthorization("expense:manage")
            .WithName(nameof(CreateExpense))
            .WithSummary("Create expense")
            .WithDescription("Create new expense details")
            .Produces<Create.Response>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder UpdateExpense(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut(ExpensesEndpoints.Update, Update.Handler.Invoke)
            .RequireAuthorization("expense:manage")
            .WithName(nameof(UpdateExpense))
            .WithSummary("Update expense")
            .WithDescription("Updates existing expense details")
            .Produces<Update.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder DeleteExpense(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapDelete(ExpensesEndpoints.Delete, Delete.Handler.Invoke)
            .RequireAuthorization("expense:manage")
            .WithName(nameof(DeleteExpense))
            .WithSummary("Delete expense")
            .WithDescription("Deletes an existing expense")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder RenewExpenses(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(ExpensesEndpoints.Renew, Renew.Handler.Invoke)
            .RequireAuthorization("expense:manage")
            .WithName(nameof(RenewExpenses))
            .WithSummary("Renew expenses")
            .WithDescription("Renews selected expenses")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder ToggleExcludeExpenses(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(ExpensesEndpoints.ToggleExclude, Handler.Invoke)
            .RequireAuthorization("expense:manage")
            .WithName(nameof(ToggleExcludeExpenses))
            .WithSummary("Toggle exclude expenses")
            .WithDescription("Toggles the 'exclude from calculations' status of selected expenses")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }
}
