using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Pot.AspNetCore.Features.Expenses.Extensions;

internal static class RouteGroupBuilderExtensions
{
    // TODO: Update ProducesProblem() across all routes

    public static RouteGroupBuilder GetAllExpenses(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(ExpensesEndpoints.GetAll, GetAll.Handler.Invoke)
            .WithName(nameof(GetAllExpenses))
            .WithSummary("Get all expenses")
            .WithDescription("Get all expense details")
            .WithTags("Expenses");

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder GetExpense(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(ExpensesEndpoints.Get, Get.Handler.Invoke)
            .WithName(nameof(GetExpense))
            .WithSummary("Get expense")
            .WithDescription("Get details for an existing expense")
            .WithTags("Expenses");

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder CreateExpense(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(ExpensesEndpoints.Create, Create.Handler.Invoke)
            .WithName(nameof(CreateExpense))
            .WithSummary("Create expense")
            .WithDescription("Create new expense details")
            .WithTags("Expenses")
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder UpdateExpense(this RouteGroupBuilder routeGroupBuilder)
    {
        //routeGroupBuilder
        //    .MapPut(ExpensesEndpoints.Update, Update.Handler.Invoke)
        //    .WithName(nameof(UpdateExpense))
        //    .WithSummary("Update expense")
        //    .WithDescription("Updates existing expense details")
        //    .WithTags("Expenses")
        //    .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder DeleteExpense(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapDelete(ExpensesEndpoints.Delete, Delete.Handler.Invoke)
            .WithName(nameof(DeleteExpense))
            .WithSummary("Delete expense")
            .WithDescription("Deletes existing expense details")
            .WithTags("Expenses")
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder ImportExpenses(this RouteGroupBuilder routeGroupBuilder, long maxImportPayloadBytes)
    {
        routeGroupBuilder
            .MapPost("/import", Import.Handler.Invoke)
            .WithName(nameof(ImportExpenses))
            .WithSummary("Import Expenses")
            .WithDescription("Import new / update existing expense details")
            .WithTags("Expenses", "Import")
            .WithMetadata(new RequestSizeLimitAttribute(maxImportPayloadBytes)) // Will raise 413 Payload Too Large if the file exceeds this limit
            .DisableAntiforgery()
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

        return routeGroupBuilder;
    }
}
