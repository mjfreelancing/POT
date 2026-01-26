using FluentAssertions;
using Pot.Data.Entities;
using Pot.Data.Specifications;
using Pot.TestUtils;

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

    public class IsSameBsbNumber : AccountSpecificationsFixture
    {
        [Fact]
        public void Should_Return_True_When_Bsb_And_Number_Match()
        {
            var bsb = "123-456";
            var number = "12345678";
            var account = CreateAccount();

            account.Bsb = bsb;
            account.Number = number;

            var specification = AccountSpecifications.IsSameBsbNumber(bsb, number);
            var result = specification.IsSatisfiedBy(account);

            result.Should().BeTrue();
        }

        [Fact]
        public void Should_Return_False_When_Bsb_Does_Not_Match()
        {
            var bsb = "123-456";
            var number = "12345678";
            var account = CreateAccount();

            account.Bsb = "999-999";
            account.Number = number;

            var specification = AccountSpecifications.IsSameBsbNumber(bsb, number);
            var result = specification.IsSatisfiedBy(account);

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_Return_False_When_Number_Does_Not_Match()
        {
            var bsb = "123-456";
            var number = "12345678";
            var account = CreateAccount();

            account.Bsb = bsb;
            account.Number = "99999999";

            var specification = AccountSpecifications.IsSameBsbNumber(bsb, number);
            var result = specification.IsSatisfiedBy(account);

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_Return_False_When_Both_Bsb_And_Number_Do_Not_Match()
        {
            var bsb = "123-456";
            var number = "12345678";
            var account = CreateAccount();

            account.Bsb = "999-999";
            account.Number = "99999999";

            var specification = AccountSpecifications.IsSameBsbNumber(bsb, number);
            var result = specification.IsSatisfiedBy(account);

            result.Should().BeFalse();
        }
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

            result.Should().BeTrue();
        }

        [Fact]
        public void Should_Return_False_When_Description_Does_Not_Match()
        {
            var description = "Savings Account";
            var account = CreateAccount();

            account.Description = "Different Account";

            var specification = AccountSpecifications.IsSameDescription(description);
            var result = specification.IsSatisfiedBy(account);

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_Be_Case_Sensitive()
        {
            var description = "Savings Account";
            var account = CreateAccount();

            account.Description = "SAVINGS ACCOUNT";

            var specification = AccountSpecifications.IsSameDescription(description);
            var result = specification.IsSatisfiedBy(account);

            result.Should().BeFalse();
        }
    }
}
