using Pot.AspNetCore.Features.Accounts.Create.Services;
using Pot.AspNetCore.Features.Accounts.Import.Services;
using Pot.AspNetCore.Features.Accounts.Update.Services;
using Pot.AspNetCore.Features.Accounts.Update.Services.PreSave;
using Pot.Data.Extensions;

namespace Pot.AspNetCore.Features.Accounts.Extensions;

internal static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddAccountServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddAccountRepositories();

        // TODO: AddAccountServices()
        builder.Services.AddScoped<IImportAccountService, ImportAccountService>();
        builder.Services.AddScoped<ICreateAccountService, CreateAccountService>();
        builder.Services.AddScoped<IUpdateAccountService, UpdateAccountService>();

        builder.Services.AddScoped<IPreUpdateChecker, PreUpdateChecker>();

        return builder;
    }
}
