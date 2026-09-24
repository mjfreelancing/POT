using Pot.Data.Entities;
using Pot.Data.Specifications;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Specifications;

public class AccountSpecificationsFixture : PotFixtureBase
{
    private readonly SiteEntity _site;

    public AccountSpecificationsFixture()
    {
        _site = EntityFactory.CreateSite();
    }

    private AccountEntity CreateAccount()
    {
        return EntityFactory.CreateAccount(_site, "Test Account", 0.0d);
    }

    public class IsSameDescription : AccountSpecificationsFixture
    {
        [Fact]
        public void Should_Return_True_When_Description_Matches()
        {
            var description = "Savings Account";
            var account = CreateAccount();

            account.Description = description;

            var specification = AccountSpecifications.IsSameDescription(description);
            var result = specification.IsSatisfiedBy(account);

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_False_When_Description_Does_Not_Match()
        {
            var description = "Savings Account";
            var account = CreateAccount();

            account.Description = "Different Account";

            var specification = AccountSpecifications.IsSameDescription(description);
            var result = specification.IsSatisfiedBy(account);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Be_Case_Sensitive()
        {
            var description = "Savings Account";
            var account = CreateAccount();

            account.Description = "SAVINGS ACCOUNT";

            var specification = AccountSpecifications.IsSameDescription(description);
            var result = specification.IsSatisfiedBy(account);

            result.ShouldBeFalse();
        }
    }
}
