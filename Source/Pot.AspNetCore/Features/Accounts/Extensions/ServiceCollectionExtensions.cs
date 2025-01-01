using Pot.AspNetCore.Features.Accounts.Import.Repository;

namespace Pot.AspNetCore.Features.Accounts.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccountServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountImportRepository, AccountImportRepository>();

        return services;
    }
}
