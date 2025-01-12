using Microsoft.AspNetCore.Mvc;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using Pot.Data.Dtos;

namespace Pot.AspNetCore.Features.Accounts.Extensions;

internal static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder GetAllAccounts(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet("", GetAll.Handler.Invoke)
            .Produces<List<AccountDto>>();

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder ImportAccounts(this RouteGroupBuilder routeGroupBuilder, long maxImportPayloadBytes)
    {
        routeGroupBuilder
            .MapPost("/import", Import.Handler.Invoke)
            .WithMetadata(new RequestSizeLimitAttribute(maxImportPayloadBytes)) // Will raise 413 Payload Too Large if the file exceeds this limit
            .DisableAntiforgery()
            .Produces<ImportResult>();

        return routeGroupBuilder;
    }
}
