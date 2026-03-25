using AllOverIt.Patterns.Specification;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;

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

    public static ILinqSpecification<ExpenseEntity> IsInAccountSet(Guid[] accountRowIds)
    {
        return LinqSpecification<ExpenseEntity>.Create(expense => accountRowIds.Contains(expense.Account.RowId));
    }

    public static ILinqSpecification<ExpenseEntity> RequiresRenewal(DateOnly asOfDate)
    {
        return LinqSpecification<ExpenseEntity>.Create(expense =>
            // Must be incluided
            !expense.ExcludeFromCalcs &&

            // We never renew one-time expenses
            expense.Frequency != Frequency.OneTime &&

            // Must be due on or before the asOfDate and not ended before the asOfDate
            // If there's no end date, then the expense is always renewable as long as it's due on or before the asOfDate
            expense.NextDue <= asOfDate && (!expense.EndDate.HasValue || expense.EndDate > asOfDate));
    }
}