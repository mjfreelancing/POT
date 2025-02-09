using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Pot.AspNetCore.Features.Accounts.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetAllAccounts(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet("", GetAll.Handler.Invoke)
            .WithName(nameof(GetAllAccounts))
            .WithSummary("Get all accounts")
            .WithDescription("Get all account details")
            .WithTags("Accounts");

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder GetAccount(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet("/{id}", Get.Handler.Invoke)
            .WithName(nameof(GetAccount))
            .WithSummary("Get account")
            .WithDescription("Get details for an existing account")
            .WithTags("Accounts")
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder CreateAccount(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost("", Create.Handler.Invoke)
            .WithName(nameof(CreateAccount))
            .WithSummary("Create account")
            .WithDescription("Create new account details")
            .WithTags("Accounts")
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder UpdateAccount(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPut("", Update.Handler.Invoke)
            .WithName(nameof(UpdateAccount))
            .WithSummary("Update account")
            .WithDescription("Updates existing account details")
            .WithTags("Accounts")
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder DeleteAccount(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapDelete("/{id}", Delete.Handler.Invoke)
            .WithName(nameof(DeleteAccount))
            .WithSummary("Delete account")
            .WithDescription("Deletes existing account details")
            .WithTags("Accounts")
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder ImportAccounts(this RouteGroupBuilder routeGroupBuilder, long maxImportPayloadBytes)
    {
        routeGroupBuilder
            .MapPost("/import", Import.Handler.Invoke)
            .WithName(nameof(ImportAccounts))
            .WithSummary("Import Accounts")
            .WithDescription("Import new / update existing account details")
            .WithTags("Accounts", "Import")
            .WithMetadata(new RequestSizeLimitAttribute(maxImportPayloadBytes)) // Will raise 413 Payload Too Large if the file exceeds this limit
            .DisableAntiforgery()
            .ProducesProblem((int)HttpStatusCode.UnprocessableEntity);

        return routeGroupBuilder;
    }
}
