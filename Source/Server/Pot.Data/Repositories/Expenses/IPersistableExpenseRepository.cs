using Pot.Data.Entities;

namespace Pot.Data.Repositories.Expenses;

public interface IPersistableExpenseRepository : IExpenseRepository, IPersistableRepository<ExpenseEntity>;
