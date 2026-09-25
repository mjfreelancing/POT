using Pot.Data.Entities;
using Pot.Data.Extensions;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Extensions;

public class ExpenseEntityExtensionsFixture : PotFixtureBase
{
    private readonly SiteEntity _site;
    private readonly AccountEntity _account;

    public ExpenseEntityExtensionsFixture()
    {
        _site = EntityFactory.CreateSite();
        _account = EntityFactory.CreateAccount(_site, "Test Account", 1000.0);
    }

    public class Balance : ExpenseEntityExtensionsFixture
    {
        [Fact]
        public void Should_Return_The_Amount_Minus_The_Accrued_Value()
        {
            var expense = CreateExpense(amount: 100.0, accrued: 30.0);

            expense.Balance().ShouldBe(70.0);
        }

        [Fact]
        public void Should_Return_The_Amount_When_Nothing_Has_Accrued()
        {
            var expense = CreateExpense(amount: 100.0, accrued: 0.0);

            expense.Balance().ShouldBe(100.0);
        }
    }

    public class DaysDueFrom : ExpenseEntityExtensionsFixture
    {
        [Fact]
        public void Should_Return_The_Days_Until_The_Next_Due_Date()
        {
            var currentDate = new DateOnly(2025, 1, 10);
            var expense = CreateExpense(nextDue: "2025-01-15");

            expense.DaysDueFrom(currentDate).ShouldBe(5);
        }

        [Fact]
        public void Should_Return_Zero_When_The_Next_Due_Date_Is_Today()
        {
            var currentDate = new DateOnly(2025, 1, 10);
            var expense = CreateExpense(nextDue: "2025-01-10");

            expense.DaysDueFrom(currentDate).ShouldBe(0);
        }

        [Fact]
        public void Should_Return_Zero_When_The_Next_Due_Date_Is_In_The_Past()
        {
            var currentDate = new DateOnly(2025, 1, 10);
            var expense = CreateExpense(nextDue: "2025-01-05");

            expense.DaysDueFrom(currentDate).ShouldBe(0);
        }
    }

    public class DaysFromAccrualStart : ExpenseEntityExtensionsFixture
    {
        [Fact]
        public void Should_Return_The_Days_Since_The_Accrual_Start()
        {
            var currentDate = new DateOnly(2025, 1, 10);
            var expense = CreateExpense(accrualStart: "2025-01-01");

            expense.DaysFromAccrualStart(currentDate).ShouldBe(9);
        }

        [Fact]
        public void Should_Return_Zero_When_The_Accrual_Start_Is_Not_Set()
        {
            var currentDate = new DateOnly(2025, 1, 10);
            var expense = CreateExpense(accrualStart: "2025-01-01");
            expense.AccrualStart = null;

            expense.DaysFromAccrualStart(currentDate).ShouldBe(0);
        }
    }

    public class DailyBalance : ExpenseEntityExtensionsFixture
    {
        [Fact]
        public void Should_Divide_The_Balance_By_The_Days_Due()
        {
            var currentDate = new DateOnly(2025, 1, 10);
            var expense = CreateExpense(amount: 100.0, accrued: 0.0, nextDue: "2025-01-15");

            expense.DailyBalance(currentDate).ShouldBe(20.0);
        }

        [Fact]
        public void Should_Use_A_Single_Day_When_The_Expense_Is_Already_Due()
        {
            var currentDate = new DateOnly(2025, 1, 10);
            var expense = CreateExpense(amount: 100.0, accrued: 25.0, nextDue: "2025-01-10");

            expense.DailyBalance(currentDate).ShouldBe(75.0);
        }
    }

    public class DailyAccrual : ExpenseEntityExtensionsFixture
    {
        [Fact]
        public void Should_Return_Zero_When_The_Accrual_Start_Is_Not_Set()
        {
            var expense = CreateExpense(amount: 100.0, accrualStart: "2025-01-01", nextDue: "2025-01-11");
            expense.AccrualStart = null;

            expense.DailyAccrual().ShouldBe(0.0);
        }

        [Fact]
        public void Should_Divide_The_Amount_By_The_Days_Between_Accrual_Start_And_Next_Due()
        {
            var expense = CreateExpense(amount: 100.0, accrualStart: "2025-01-01", nextDue: "2025-01-11");

            expense.DailyAccrual().ShouldBe(10.0);
        }

        [Fact]
        public void Should_Use_A_Single_Day_When_Accrual_Start_Equals_Next_Due()
        {
            var expense = CreateExpense(amount: 100.0, accrualStart: "2025-01-11", nextDue: "2025-01-11");

            expense.DailyAccrual().ShouldBe(100.0);
        }
    }

    private ExpenseEntity CreateExpense(double amount = 100.0, double accrued = 0.0, string accrualStart = "2025-01-01",
        string nextDue = "2025-02-01")
    {
        var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", amount, accrualStart, nextDue, null, Frequency.Months, 1);
        expense.Accrued = accrued;

        return expense;
    }
}
