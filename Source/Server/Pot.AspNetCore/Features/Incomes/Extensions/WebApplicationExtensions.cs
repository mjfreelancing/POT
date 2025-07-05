namespace Pot.AspNetCore.Features.Incomes.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddIncomeEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Income Routes]"))
        {
            app.Logger.LogInformation("Adding income endpoints");

            app.MapGroup(IncomesEndpoints.Group)
                //.RequireAuthorization()
                .WithTags(IncomesEndpoints.Tag)
                .GetAllIncomes()
                .GetIncome()
                .CreateIncome()
                .UpdateIncome()
                .DeleteIncome()
                .RenewIncomes();
        }

        return app;
    }
}
