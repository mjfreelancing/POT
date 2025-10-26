namespace Pot.AspNetCore.Features.Sites.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddSiteEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Site Routes]"))
        {
            app.Logger.LogInformation("Adding site endpoints");

            var group = app
                .MapGroup(SitesEndpoints.Group)
                .WithTags(SitesEndpoints.Tag)
                .UpdateSite();
        }

        return app;
    }
}
