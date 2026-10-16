using Pot.AspNetCore.Concerns.RateLimiting;
using Pot.AspNetCore.Extensions;

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
                .ProducesStandardProblems()
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
