using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Pot.AspNetCore.Features.Maintenance.Extensions;

internal static class RouteGroupBuilderExtensions
{
    private const long MaxImportPayloadBytes = 10 * 1024 * 1024;

    public static RouteGroupBuilder ExportData(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(MaintenanceEndpoints.Export, Export.Handler.Invoke)
            .RequireAuthorization("maintenance:export")
            .WithName(nameof(Export))
            .WithSummary("Export data")
            .WithDescription("Export Accounts, Incomes, and Expense data")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }

    public static RouteGroupBuilder ImportData(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapPost(MaintenanceEndpoints.Import, Import.Handler.Invoke)
            .RequireAuthorization("maintenance:import")
            .WithName(nameof(Import))
            .WithSummary("Import data")
            .WithDescription("Import Accounts, Incomes, and Expense data")
            .WithMetadata(new RequestSizeLimitAttribute(MaxImportPayloadBytes)) // Will raise 413 Payload Too Large if the file exceeds this limit
            .DisableAntiforgery()
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.RequestEntityTooLarge)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
