namespace Pot.AspNetCore.Features.Accruals.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddAccrualsEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Accruals Routes]"))
        {
            app.Logger.LogInformation("Adding accruals endpoints");

            app.MapGroup(AccrualsEndpoints.Group)
                .WithTags(AccrualsEndpoints.Tag)
                .GetStatus()
                .AccrueAccountExpenses();
        }

        return app;
    }
}
