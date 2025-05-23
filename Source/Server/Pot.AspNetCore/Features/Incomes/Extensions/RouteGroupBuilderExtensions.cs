using System.Net;

namespace Pot.AspNetCore.Features.Incomes.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetAllIncomes(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet("", GetAll.Handler.Invoke)
            .WithName(nameof(GetAllIncomes))
            .WithSummary("Get all incomes")
            .WithDescription("Get all income details")
            .WithTags("Incomes");

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder GetIncome(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet("/{id}", Get.Handler.Invoke)
            .WithName(nameof(GetIncome))
            .WithSummary("Get income")
            .WithDescription("Get details for an existing income source")
            .WithTags("Incomes")
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

        return routeGroupBuilder;
    }

    // Use the code below as a template


    //public static RouteGroupBuilder CreateAccount(this RouteGroupBuilder routeGroupBuilder)
    //{
    //    routeGroupBuilder
    //        .MapPost("", Create.Handler.Invoke)
    //        .WithName(nameof(CreateAccount))
    //        .WithSummary("Create account")
    //        .WithDescription("Create new account details")
    //        .WithTags("Accounts")
    //        .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

    //    return routeGroupBuilder;
    //}

    //public static RouteGroupBuilder UpdateAccount(this RouteGroupBuilder routeGroupBuilder)
    //{
    //    routeGroupBuilder
    //        .MapPut("", Update.Handler.Invoke)
    //        .WithName(nameof(UpdateAccount))
    //        .WithSummary("Update account")
    //        .WithDescription("Updates existing account details")
    //        .WithTags("Accounts")
    //        .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

    //    return routeGroupBuilder;
    //}

    //public static RouteGroupBuilder DeleteAccount(this RouteGroupBuilder routeGroupBuilder)
    //{
    //    routeGroupBuilder
    //        .MapDelete("/{id}", Delete.Handler.Invoke)
    //        .WithName(nameof(DeleteAccount))
    //        .WithSummary("Delete account")
    //        .WithDescription("Deletes existing account details")
    //        .WithTags("Accounts")
    //        .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

    //    return routeGroupBuilder;
    //}
}
