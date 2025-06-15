namespace Pot.AspNetCore.Features.Projections.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddProjectionsEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Projection Routes]"))
        {
            app.Logger.LogInformation("Adding projections endpoints");

            var group = app
                .MapGroup(ProjectionsEndpoints.Group)
                .WithTags(ProjectionsEndpoints.Tag)
                .GetProjections();
        }

        return app;
    }
}
