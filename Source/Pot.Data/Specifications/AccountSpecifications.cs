using AllOverIt.Patterns.Specification;
using Pot.Data.Entities;

namespace Pot.Data.Specifications;

public static class AccountSpecifications
{
    public static ILinqSpecification<AccountEntity> IsSameBsbNumber(string bsb, string number)
    {
        return LinqSpecification<AccountEntity>.Create(account => account.Bsb == bsb && account.Number == number);
    }

    public static ILinqSpecification<AccountEntity> IsSameDescription(string description)
    {
        return LinqSpecification<AccountEntity>.Create(account => account.Description == description);
    }
}