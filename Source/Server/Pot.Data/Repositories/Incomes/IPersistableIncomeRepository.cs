using Pot.Data.Entities;

namespace Pot.Data.Repositories.Incomes;

public interface IPersistableIncomeRepository : IIncomeRepository, IPersistableRepository<IncomeEntity>;
