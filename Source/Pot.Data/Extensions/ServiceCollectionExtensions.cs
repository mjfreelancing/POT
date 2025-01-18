using Microsoft.Extensions.DependencyInjection;
using Pot.Data.Repositories.Accounts;

namespace Pot.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccountRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAccountRepository, AccountRepository>();

        return services;
    }
}