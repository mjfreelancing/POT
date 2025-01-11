using Pot.AspNetCore.Features.Accounts.Import.Repository;

namespace Pot.AspNetCore.Features.Accounts.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddAccountServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAccountImportRepository, AccountImportRepository>();

        return builder;
    }
}
