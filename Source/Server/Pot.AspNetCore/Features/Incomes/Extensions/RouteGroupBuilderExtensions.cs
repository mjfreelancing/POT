using System.Net;

namespace Pot.AspNetCore.Features.Incomes.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetAllIncomes(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(IncomesEndpoints.GetAll, GetAll.Handler.Invoke)
            .WithName(nameof(GetAllIncomes))
            .WithSummary("Get all incomes")
            .WithDescription("Get all income details")
            .WithTags("Incomes")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder GetIncome(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(IncomesEndpoints.Get, Get.Handler.Invoke)
            .WithName(nameof(GetIncome))
            .WithSummary("Get income")
            .WithDescription("Get details for an existing income source")
            .WithTags("Incomes")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder CreateIncome(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(IncomesEndpoints.Create, Create.Handler.Invoke)
            .WithName(nameof(CreateIncome))
            .WithSummary("Create income")
            .WithDescription("Create a new income source")
            .WithTags("Incomes")
            .ProducesProblem((int)HttpStatusCode.Created)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder UpdateIncome(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut(IncomesEndpoints.Update, Update.Handler.Invoke)
            .WithName(nameof(UpdateIncome))
            .WithSummary("Update income")
            .WithDescription("Updates existing income details")
            .WithTags("Incomes")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder DeleteIncome(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapDelete(IncomesEndpoints.Delete, Delete.Handler.Invoke)
            .WithName(nameof(DeleteIncome))
            .WithSummary("Delete income")
            .WithDescription("Deletes existing income details")
            .WithTags("Incomes")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.NotFound)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
