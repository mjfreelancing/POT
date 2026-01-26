using FluentAssertions;
using Pot.Data.Entities;
using Pot.Data.Specifications;
using Pot.Shared.Enumerations;
using Pot.TestUtils;

namespace Pot.Data.Tests.Specifications;

public class IncomeSpecificationsFixture : PotFixtureBase
{
    private readonly SiteEntity _site;
    private readonly AccountEntity _account;

    public IncomeSpecificationsFixture()
    {
        _site = EntityFactory.CreateSite();
        _account = EntityFactory.CreateAccount(_site, "Test Account", 0.0d);
    }

    private IncomeEntity CreateIncome(string description)
    {
        return EntityFactory.CreateIncome(
            _account,
            excludeFromCalc: false,
            description: description,
            amount: 1000.0d,
            nextDue: "2025-02-01",
            endDate: null,
            frequency: Frequency.Months,
            frequencyCount: 1
        );
    }

    public class IsSameDescription : IncomeSpecificationsFixture
    {
        [Fact]
        public void Should_Return_True_When_AccountId_And_Description_Match()
        {
            var description = "Salary";
            var income = CreateIncome(description);

            var specification = IncomeSpecifications.IsSameDescription(_account.Id, description);
            var result = specification.IsSatisfiedBy(income);

            result.Should().BeTrue();
        }

        [Fact]
        public void Should_Return_False_When_AccountId_Does_Not_Match()
        {
            var description = "Salary";
            var income = CreateIncome(description);
            var differentAccountId = _account.Id + 999;

            var specification = IncomeSpecifications.IsSameDescription(differentAccountId, description);
            var result = specification.IsSatisfiedBy(income);

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_Return_False_When_Description_Does_Not_Match()
        {
            var description = "Salary";
            var income = CreateIncome("Bonus");

            var specification = IncomeSpecifications.IsSameDescription(_account.Id, description);
            var result = specification.IsSatisfiedBy(income);

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_Return_False_When_Both_AccountId_And_Description_Do_Not_Match()
        {
            var description = "Salary";
            var income = CreateIncome("Bonus");
            var differentAccountId = _account.Id + 999;

            var specification = IncomeSpecifications.IsSameDescription(differentAccountId, description);
            var result = specification.IsSatisfiedBy(income);

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_Be_Case_Sensitive()
        {
            var description = "Salary";
            var income = CreateIncome("SALARY");

            var specification = IncomeSpecifications.IsSameDescription(_account.Id, description);
            var result = specification.IsSatisfiedBy(income);

            result.Should().BeFalse();
        }
    }
}
