using Pot.Maui.Domain.Accounts.Repository;
using Pot.Maui.Domain.Expenses.Repository;

namespace Pot.Maui.Domain.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddSingleton<IAccountRepository, AccountRepository>();
            services.AddSingleton<IExpenseRepository, ExpenseRepository>();

            return services;
        }
    }
}
