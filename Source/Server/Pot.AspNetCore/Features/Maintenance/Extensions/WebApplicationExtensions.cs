namespace Pot.AspNetCore.Features.Maintenance.Extensions;

internal static class WebApplicationExtensions
{
    private const long MaxImportPayloadBytes = 10 * 1024 * 1024;

    public static WebApplication AddMaintenanceEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Maintenance Routes]"))
        {
            app.Logger.LogInformation("Adding maintenance endpoints");

            var group = app
                .MapGroup(MaintenanceEndpoints.Group)
                .WithTags(MaintenanceEndpoints.Tag)
                .ExportData()
                .ImportData(MaxImportPayloadBytes)
                .CreateRsaKeys();
        }

        return app;
    }
}
