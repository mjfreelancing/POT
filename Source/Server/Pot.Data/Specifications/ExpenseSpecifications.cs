using AllOverIt.Patterns.Specification;
using Pot.Data.Entities;

namespace Pot.Data.Specifications;

public static class ExpenseSpecifications
{
    public static ILinqSpecification<ExpenseEntity> IsSameDescription(int accountId, string description)
    {
        return LinqSpecification<ExpenseEntity>.Create(expense => expense.Account.Id == accountId && expense.Description == description);
    }

    public static ILinqSpecification<ExpenseEntity> RequiresAccrualUpdate(DateOnly asOfDate)
    {
        return LinqSpecification<ExpenseEntity>.Create(expense => expense.AccruedIsDirty || expense.LastAccruedUpdate == null || expense.LastAccruedUpdate < asOfDate);
    }
}