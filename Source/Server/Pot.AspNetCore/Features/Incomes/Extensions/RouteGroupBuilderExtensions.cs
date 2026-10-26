using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Incomes.ToggleExclude;

namespace Pot.AspNetCore.Features.Incomes.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetAllIncomes(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(IncomesEndpoints.GetAll, GetAll.Handler.Invoke)
            .RequireAuthorization("income:view")
            .WithName(nameof(GetAllIncomes))
            .WithSummary("Get all incomes")
            .WithDescription("Get all income details")
            .Produces<GetAll.Response[]>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder GetIncome(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(IncomesEndpoints.Get, Get.Handler.Invoke)
            .RequireAuthorization("income:view")
            .WithName(nameof(GetIncome))
            .WithSummary("Get income")
            .WithDescription("Get details for an existing income source")
            .Produces<Get.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder CreateIncome(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(IncomesEndpoints.Create, Create.Handler.Invoke)
            .RequireAuthorization("income:manage")
            .WithName(nameof(CreateIncome))
            .WithSummary("Create income")
            .WithDescription("Create a new income source")
            .Produces<Create.Response>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesRequestBodyProblems();

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder UpdateIncome(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut(IncomesEndpoints.Update, Update.Handler.Invoke)
            .RequireAuthorization("income:manage")
            .WithName(nameof(UpdateIncome))
            .WithSummary("Update income")
            .WithDescription("Updates existing income details")
            .Produces<Update.Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesStaleEntityTagConflict()
            .ProducesRequestBodyProblems();

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder DeleteIncome(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapDelete(IncomesEndpoints.Delete, Delete.Handler.Invoke)
            .RequireAuthorization("income:manage")
            .WithName(nameof(DeleteIncome))
            .WithSummary("Delete income")
            .WithDescription("Deletes existing income details")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder RenewIncomes(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(IncomesEndpoints.Renew, Renew.Handler.Invoke)
            .RequireAuthorization("income:manage")
            .WithName(nameof(RenewIncomes))
            .WithSummary("Renew incomes")
            .WithDescription("Renews selected incomes")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesRequestBodyProblems();

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder ToggleExcludeIncomes(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(IncomesEndpoints.ToggleExclude, Handler.Invoke)
            .RequireAuthorization("income:manage")
            .WithName(nameof(ToggleExcludeIncomes))
            .WithSummary("Toggle exclude incomes")
            .WithDescription("Toggles the 'exclude from calculations' status of selected incomes")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesRequestBodyProblems();

        return routeGroupBuilder;
    }
}
