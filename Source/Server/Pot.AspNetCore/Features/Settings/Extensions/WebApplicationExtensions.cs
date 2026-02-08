using Pot.AspNetCore.Concerns.RateLimiting;

namespace Pot.AspNetCore.Features.Settings.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddSettingsEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Settings Routes]"))
        {
            app.Logger.LogInformation("Adding settings endpoints");

            app.MapGroup(SettingsEndpoints.Group)
               .WithTags(SettingsEndpoints.Tag)
               .RequireRateLimiting(RateLimiterPolicy.Chained)
               .GetAllSettings()
               .UpdateSettings();
        }

        return app;
    }
}
