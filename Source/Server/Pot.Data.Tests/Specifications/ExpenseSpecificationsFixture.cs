using Pot.Data.Entities;
using Pot.Data.Specifications;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Specifications;

public class ExpenseSpecificationsFixture : PotFixtureBase
{
    private readonly SiteEntity _site;
    private readonly AccountEntity _account;

    public ExpenseSpecificationsFixture()
    {
        _site = EntityFactory.CreateSite();
        _account = EntityFactory.CreateAccount(_site, "Test Account", 0.0d);
    }

    private ExpenseEntity CreateExpense(string description)
    {
        return EntityFactory.CreateExpense(
            _account,
            excludeFromCalc: false,
            description: description,
            amount: 100.0d,
            accrualStart: "2025-01-01",
            nextDue: "2025-02-01",
            endDate: null,
            frequency: Frequency.Months,
            frequencyCount: 1
        );
    }

    public class IsSameDescription : ExpenseSpecificationsFixture
    {
        [Fact]
        public void Should_Return_True_When_AccountId_And_Description_Match()
        {
            var description = "Rent";
            var expense = CreateExpense(description);

            var specification = ExpenseSpecifications.IsSameDescription(_account.Id, description);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_False_When_AccountId_Does_Not_Match()
        {
            var description = "Rent";
            var expense = CreateExpense(description);
            var differentAccountId = _account.Id + 999;

            var specification = ExpenseSpecifications.IsSameDescription(differentAccountId, description);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_Description_Does_Not_Match()
        {
            var description = "Rent";
            var expense = CreateExpense("Utilities");

            var specification = ExpenseSpecifications.IsSameDescription(_account.Id, description);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_Both_AccountId_And_Description_Do_Not_Match()
        {
            var description = "Rent";
            var expense = CreateExpense("Utilities");
            var differentAccountId = _account.Id + 999;

            var specification = ExpenseSpecifications.IsSameDescription(differentAccountId, description);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Be_Case_Sensitive()
        {
            var description = "Rent";
            var expense = CreateExpense("RENT");

            var specification = ExpenseSpecifications.IsSameDescription(_account.Id, description);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeFalse();
        }
    }

    public class IsInAccountSet : ExpenseSpecificationsFixture
    {
        [Fact]
        public void Should_Return_True_When_Expense_Account_Is_In_Set()
        {
            var expense = CreateExpense("Rent");

            var specification = ExpenseSpecifications.IsInAccountSet([_account.RowId]);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_False_When_Expense_Account_Is_Not_In_Set()
        {
            var otherAccount = EntityFactory.CreateAccount(_site, "Other Account", 0.0d);
            var expense = EntityFactory.CreateExpense(
                otherAccount,
                excludeFromCalc: false,
                description: "Rent",
                amount: 100.0d,
                accrualStart: "2025-01-01",
                nextDue: "2025-02-01",
                endDate: null,
                frequency: Frequency.Months,
                frequencyCount: 1);

            var specification = ExpenseSpecifications.IsInAccountSet([_account.RowId]);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeFalse();
        }
    }

    public class RequiresRenewal : ExpenseSpecificationsFixture
    {
        [Fact]
        public void Should_Return_True_When_Expense_Is_Eligible_For_Renewal()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var expense = EntityFactory.CreateExpense(_account, false, "Rent", 100.0d, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);

            var specification = ExpenseSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_False_When_EndDate_Equals_AsOfDate()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var expense = EntityFactory.CreateExpense(_account, false, "Rent", 100.0d, "2025-01-01", "2025-01-10", "2025-01-20", Frequency.Weeks, 1);

            var specification = ExpenseSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_Frequency_Is_OneTime()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var expense = EntityFactory.CreateExpense(_account, false, "One Time", 100.0d, "2025-01-01", "2025-01-10", null, Frequency.OneTime, 1);

            var specification = ExpenseSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_ExcludeFromCalcs_Is_True()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var expense = EntityFactory.CreateExpense(_account, true, "Excluded", 100.0d, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);

            var specification = ExpenseSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_NextDue_Is_After_AsOfDate()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var expense = EntityFactory.CreateExpense(_account, false, "Future Due", 100.0d, "2025-01-01", "2025-01-21", null, Frequency.Weeks, 1);

            var specification = ExpenseSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_EndDate_Is_Before_AsOfDate()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var expense = EntityFactory.CreateExpense(_account, false, "Ended", 100.0d, "2025-01-01", "2025-01-10", "2025-01-19", Frequency.Weeks, 1);

            var specification = ExpenseSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_Multiple_Negative_Conditions_Apply()
        {
            var asOfDate = new DateOnly(2025, 1, 20);
            var expense = EntityFactory.CreateExpense(_account, true, "Invalid", 100.0d, "2025-01-01", "2025-01-25", "2025-01-20", Frequency.OneTime, 1);

            var specification = ExpenseSpecifications.RequiresRenewal(asOfDate);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeFalse();
        }
    }

    public class RequiresAccrualUpdate : ExpenseSpecificationsFixture
    {
        [Fact]
        public void Should_Return_True_When_AccruedIsDirty_Is_True()
        {
            var asOfDate = new DateOnly(2025, 1, 15);

            var expense = CreateExpense("Rent");
            expense.AccruedIsDirty = true;
            expense.LastAccruedUpdate = asOfDate;

            var specification = ExpenseSpecifications.RequiresAccrualUpdate(asOfDate);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_True_When_LastAccruedUpdate_Is_Null()
        {
            var asOfDate = new DateOnly(2025, 1, 15);
            var expense = CreateExpense("Rent");
            expense.AccruedIsDirty = false;
            expense.LastAccruedUpdate = null;

            var specification = ExpenseSpecifications.RequiresAccrualUpdate(asOfDate);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_True_When_LastAccruedUpdate_Is_Before_AsOfDate()
        {
            var asOfDate = new DateOnly(2025, 1, 15);
            var expense = CreateExpense("Rent");
            expense.AccruedIsDirty = false;
            expense.LastAccruedUpdate = new DateOnly(2025, 1, 10);

            var specification = ExpenseSpecifications.RequiresAccrualUpdate(asOfDate);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_False_When_All_Conditions_Are_False()
        {
            var asOfDate = new DateOnly(2025, 1, 15);
            var expense = CreateExpense("Rent");
            expense.AccruedIsDirty = false;
            expense.LastAccruedUpdate = new DateOnly(2025, 1, 15);

            var specification = ExpenseSpecifications.RequiresAccrualUpdate(asOfDate);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_False_When_LastAccruedUpdate_Is_After_AsOfDate()
        {
            var asOfDate = new DateOnly(2025, 1, 15);
            var expense = CreateExpense("Rent");
            expense.AccruedIsDirty = false;
            expense.LastAccruedUpdate = new DateOnly(2025, 1, 20);

            var specification = ExpenseSpecifications.RequiresAccrualUpdate(asOfDate);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeFalse();
        }

        [Fact]
        public void Should_Return_True_When_Multiple_Conditions_Are_True()
        {
            var asOfDate = new DateOnly(2025, 1, 15);
            var expense = CreateExpense("Rent");
            expense.AccruedIsDirty = true;
            expense.LastAccruedUpdate = null;

            var specification = ExpenseSpecifications.RequiresAccrualUpdate(asOfDate);
            var result = specification.IsSatisfiedBy(expense);

            result.ShouldBeTrue();
        }
    }
}
