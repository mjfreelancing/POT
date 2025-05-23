namespace Pot.AspNetCore.Features.Incomes.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication AddIncomeEndpoints(this WebApplication app)
    {
        using (app.Logger.BeginScope("[Setup Income Routes]"))
        {
            app.Logger.LogInformation("Adding income endpoints");

            app.MapGroup("api/incomes")
                //.RequireAuthorization()
                .WithTags("Incomes Api")
                .GetAllIncomes()
                //.GetIncome()
                //.CreateIncome()
                //.UpdateIncome()
                //.DeleteIncome()
                ;
        }

        return app;
    }
}
