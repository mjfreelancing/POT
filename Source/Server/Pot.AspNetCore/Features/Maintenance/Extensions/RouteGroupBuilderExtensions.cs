using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Pot.AspNetCore.Features.Maintenance.Extensions;

internal static class RouteGroupBuilderExtensions
{
    // TODO: Update ProducesProblem() across all routes

    public static RouteGroupBuilder ExportData(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(MaintenanceEndpoints.Export, Export.Handler.Invoke)
            .RequireAuthorization("maintenance:export")
            .WithName(nameof(Export))
            .WithSummary("Export data")
            .WithDescription("Export Accounts, Incomes, and Expense data")
            .WithTags("Maintenance")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder ImportData(this RouteGroupBuilder routeGroupBuilder, long maxImportPayloadBytes)
    {
        routeGroupBuilder
            .MapPost(MaintenanceEndpoints.Import, Import.Handler.Invoke)
            .RequireAuthorization("maintenance:import")
            .WithName(nameof(Import))
            .WithSummary("Import data")
            .WithDescription("Import Accounts, Incomes, and Expense data")
            .WithTags("Maintenance")
            .WithMetadata(new RequestSizeLimitAttribute(maxImportPayloadBytes)) // Will raise 413 Payload Too Large if the file exceeds this limit
            .DisableAntiforgery()
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.RequestEntityTooLarge)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder CreateRsaKeys(this RouteGroupBuilder routeGroupBuilder)
    {
        // TODO: Needs to be locked down to a super-user as this is not a public endpoint
        routeGroupBuilder
            .MapPost(MaintenanceEndpoints.RsaKeys, Rsa.Keys.Handler.Invoke)
            .RequireAuthorization("AuthenticatedUser")
            .WithName(nameof(CreateRsaKeys))
            .WithSummary("Create Rsa Keys")
            .WithDescription("Create Rsa Public/Private Keys")
            .WithTags("Maintenance")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
