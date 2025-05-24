using AllOverIt.Patterns.Specification;
using Pot.Data.Entities;

namespace Pot.Data.Specifications;

public static class IncomeSpecifications
{
    public static ILinqSpecification<IncomeEntity> IsSameDescription(string description)
    {
        return LinqSpecification<IncomeEntity>.Create(account => account.Description == description);
    }
}