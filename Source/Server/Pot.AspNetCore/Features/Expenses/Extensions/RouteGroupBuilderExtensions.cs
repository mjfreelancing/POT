using Pot.AspNetCore.Features.Expenses.ToggleExclude;
using System.Net;

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
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

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
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

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
            .ProducesProblem((int)HttpStatusCode.Created)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

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
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

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
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

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
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

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
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
