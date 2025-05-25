using AllOverIt.Patterns.Specification;
using Pot.Data.Entities;

namespace Pot.Data.Specifications;

public static class IncomeSpecifications
{
    public static ILinqSpecification<IncomeEntity> IsSameDescription(int accountId, string description)
    {
        return LinqSpecification<IncomeEntity>.Create(income => income.Account.Id == accountId && income.Description == description);
    }
}