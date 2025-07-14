namespace Pot.AspNetCore.Features.Maintenance.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddMaintenanceEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Maintenance Routes]"))
        {
            app.Logger.LogInformation("Adding maintenance endpoints");

            var group = app
                .MapGroup(MaintenanceEndpoints.Group)
                .WithTags(MaintenanceEndpoints.Tag)
                .ExportData();
        }

        return app;
    }
}
