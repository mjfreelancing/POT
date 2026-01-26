using AllOverIt.Patterns.Specification;
using Pot.Data.Entities;

namespace Pot.Data.Specifications;

public static class ExpenseSpecifications
{
    public static ILinqSpecification<ExpenseEntity> IsSameDescription(int accountId, string description)
    {
        // This appears case-insensitive but the database schema uses CITEXT for the text fields
        return LinqSpecification<ExpenseEntity>.Create(expense => expense.Account.Id == accountId && expense.Description == description);
    }

    public static ILinqSpecification<ExpenseEntity> RequiresAccrualUpdate(DateOnly asOfDate)
    {
        // Returns true if expense accruals need recalculation because:
        // 1. AccruedIsDirty = expense was renewed (NextDue/AccrualStart changed)
        // 2. LastAccruedUpdate is null = expense has never been accrued
        // 3. LastAccruedUpdate < asOfDate = accruals are stale (calculated before current date)
        return LinqSpecification<ExpenseEntity>.Create(expense => expense.AccruedIsDirty || expense.LastAccruedUpdate == null || expense.LastAccruedUpdate < asOfDate);
    }
}