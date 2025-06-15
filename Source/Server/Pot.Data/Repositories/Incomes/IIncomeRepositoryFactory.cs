using Pot.App.Concerns.DependencyInjection;

namespace Pot.Data.Repositories.Incomes;

public interface IIncomeRepositoryFactory : IPotSingletonDependency
{
    IIncomeRepository CreateIncomeRepository();
}
