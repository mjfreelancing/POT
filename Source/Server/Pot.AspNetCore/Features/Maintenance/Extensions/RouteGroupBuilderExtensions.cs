using System.Net;

namespace Pot.AspNetCore.Features.Maintenance.Extensions;

internal static class RouteGroupBuilderExtensions
{
    // TODO: Update ProducesProblem() across all routes

    public static RouteGroupBuilder ExportData(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder
            .MapGet(MaintenanceEndpoints.Export, Export.Handler.Invoke)
            .WithName(nameof(Export))
            .WithSummary("Export data")
            .WithDescription("Export Accounts, Incomes, and Expense data")
            .WithTags("Maintenance")
            .ProducesProblem((int)HttpStatusCode.OK)
            .ProducesProblem((int)HttpStatusCode.InternalServerError);

        return routeGroupBuilder;
    }
}
