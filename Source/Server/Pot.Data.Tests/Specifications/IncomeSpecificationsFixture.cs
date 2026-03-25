using Pot.Data.Entities;
using Pot.Data.Specifications;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

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

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_False_When_AccountId_Does_Not_Match()
        {
            var description = "Salary";
            var income = CreateIncome(description);
            var differentAccountId = _account.Id + 999;

            var specification = IncomeSpecifications.IsSameDescription(differentAccountId, description);
            var result = specification.IsSatisfiedBy(income);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_Description_Does_Not_Match()
        {
            var description = "Salary";
            var income = CreateIncome("Bonus");

            var specification = IncomeSpecifications.IsSameDescription(_account.Id, description);
            var result = specification.IsSatisfiedBy(income);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_Both_AccountId_And_Description_Do_Not_Match()
        {
            var description = "Salary";
            var income = CreateIncome("Bonus");
            var differentAccountId = _account.Id + 999;

            var specification = IncomeSpecifications.IsSameDescription(differentAccountId, description);
            var result = specification.IsSatisfiedBy(income);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Be_Case_Sensitive()
        {
            var description = "Salary";
            var income = CreateIncome("SALARY");

            var specification = IncomeSpecifications.IsSameDescription(_account.Id, description);
            var result = specification.IsSatisfiedBy(income);

            result.ShouldBeFalse();
        }
    }

    public class IsInAccountSet : IncomeSpecificationsFixture
    {
        [Fact]
        public void Should_Return_True_When_Income_Account_Is_In_Set()
        {
            var income = CreateIncome("Salary");

            var specification = IncomeSpecifications.IsInAccountSet([_account.RowId]);
            var result = specification.IsSatisfiedBy(income);

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_False_When_Income_Account_Is_Not_In_Set()
        {
            var otherAccount = EntityFactory.CreateAccount(_site, "Other Account", 0.0d);
            var income = EntityFactory.CreateIncome(
                otherAccount,
                excludeFromCalc: false,
                description: "Salary",
                amount: 1000.0d,
                nextDue: "2025-02-01",
                endDate: null,
                frequency: Frequency.Months,
                frequencyCount: 1);

            var specification = IncomeSpecifications.IsInAccountSet([_account.RowId]);
            var result = specification.IsSatisfiedBy(income);

            result.ShouldBeFalse();
        }
    }

    public class RequiresRenewal : IncomeSpecificationsFixture
    {
        [Fact]
        public void Should_Return_True_When_Income_Is_Eligible_For_Renewal()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var income = EntityFactory.CreateIncome(_account, false, "Salary", 1000.0d, "2025-01-10", null, Frequency.Weeks, 1);

            var specification = IncomeSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(income);

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_False_When_EndDate_Equals_AsOfDate()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var income = EntityFactory.CreateIncome(_account, false, "Salary", 1000.0d, "2025-01-10", "2025-01-20", Frequency.Weeks, 1);

            var specification = IncomeSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(income);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_Frequency_Is_OneTime()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var income = EntityFactory.CreateIncome(_account, false, "One Time", 1000.0d, "2025-01-10", null, Frequency.OneTime, 1);

            var specification = IncomeSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(income);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_ExcludeFromCalcs_Is_True()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var income = EntityFactory.CreateIncome(_account, true, "Excluded", 1000.0d, "2025-01-10", null, Frequency.Weeks, 1);

            var specification = IncomeSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(income);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_NextDue_Is_After_AsOfDate()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var income = EntityFactory.CreateIncome(_account, false, "Future Due", 1000.0d, "2025-01-21", null, Frequency.Weeks, 1);

            var specification = IncomeSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(income);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_EndDate_Is_Before_AsOfDate()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var income = EntityFactory.CreateIncome(_account, false, "Ended", 1000.0d, "2025-01-10", "2025-01-19", Frequency.Weeks, 1);

            var specification = IncomeSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(income);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_Multiple_Negative_Conditions_Apply()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var income = EntityFactory.CreateIncome(_account, true, "Invalid", 1000.0d, "2025-01-25", "2025-01-20", Frequency.OneTime, 1);

            var specification = IncomeSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(income);

            result.ShouldBeFalse();
        }
    }
}
