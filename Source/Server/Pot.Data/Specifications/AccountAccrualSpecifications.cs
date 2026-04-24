using AllOverIt.Patterns.Specification;
using Pot.Data.Entities;

namespace Pot.Data.Specifications;

public static class AccountAccrualSpecifications
{
    public static ILinqSpecification<AccountAccrualEntity> IsInAccountSet(Guid[] accountRowIds)
    {
        return LinqSpecification<AccountAccrualEntity>.Create(accountAccrual => accountRowIds.Contains(accountAccrual.Account.RowId));
    }

    public static ILinqSpecification<AccountAccrualEntity> RequiresAccrualUpdate(DateOnly asOfDate)
    {
        return LinqSpecification<AccountAccrualEntity>.Create(accountAccrual =>
            accountAccrual.AccruedIsDirty || accountAccrual.LastAccruedDate == null || accountAccrual.LastAccruedDate < asOfDate);
    }
}
