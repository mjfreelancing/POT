using Pot.AspNetCore.Features.Accounts.Import.Services;
using Pot.Data.Extensions;

namespace Pot.AspNetCore.Features.Accounts.Extensions;

internal static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddAccountServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddAccountRepositories();
        builder.Services.AddScoped<IAccountImportService, AccountImportService>();

        return builder;
    }
}
