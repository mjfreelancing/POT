using Microsoft.Extensions.DependencyInjection;

namespace Pot.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPotDbContext(this IServiceCollection services)
        {
            services.AddDbContextFactory<PotDbContext>();

            return services;
        }
    }
}