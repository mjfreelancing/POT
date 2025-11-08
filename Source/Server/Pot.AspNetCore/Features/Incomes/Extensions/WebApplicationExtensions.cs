using Pot.AspNetCore.Concerns.RateLimiting;

namespace Pot.AspNetCore.Features.Incomes.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddIncomeEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Income Routes]"))
        {
            app.Logger.LogInformation("Adding income endpoints");

            app.MapGroup(IncomesEndpoints.Group)
                .WithTags(IncomesEndpoints.Tag)
                .RequireRateLimiting(RateLimiterPolicy.Chained)
                .GetAllIncomes()
                .GetIncome()
                .CreateIncome()
                .UpdateIncome()
                .DeleteIncome()
                .RenewIncomes()
                .ToggleExcludeIncomes();
        }

        return app;
    }
}
