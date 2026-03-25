using AllOverIt.Patterns.Specification;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;

namespace Pot.Data.Specifications;

public static class IncomeSpecifications
{
    public static ILinqSpecification<IncomeEntity> IsSameDescription(int accountId, string description)
    {
        // This appears case-insensitive but the database schema uses CITEXT for the text fields
        return LinqSpecification<IncomeEntity>.Create(income => income.Account.Id == accountId && income.Description == description);
    }

    public static ILinqSpecification<IncomeEntity> IsInAccountSet(Guid[] accountRowIds)
    {
        return LinqSpecification<IncomeEntity>.Create(income => accountRowIds.Contains(income.Account.RowId));
    }

    public static ILinqSpecification<IncomeEntity> RequiresRenewal(DateOnly asOfDate)
    {
        return LinqSpecification<IncomeEntity>.Create(income =>
            // Must be incluided
            !income.ExcludeFromCalcs &&

            // We never renew one-time incomes
            income.Frequency != Frequency.OneTime &&

            // Must be due on or before the asOfDate and not ended before the asOfDate
            // If there's no end date, then the income is always renewable as long as it's due on or before the asOfDate
            income.NextDue <= asOfDate && (!income.EndDate.HasValue || income.EndDate > asOfDate));
    }
}