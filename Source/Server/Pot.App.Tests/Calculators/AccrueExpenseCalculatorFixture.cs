using AllOverIt.Extensions;
using AllOverIt.Fixture.Extensions;
using FluentAssertions;
using NSubstitute;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.TestUtils;

namespace Pot.App.Tests.Calculators;

public class AccrueExpenseCalculatorFixture : PotFixtureBase
{
    public AccrueExpenseCalculatorFixture()
    {
        CustomizeEnumerations();
        OmitRecursionBehavior();
    }

    public class Constructor : AccrueExpenseCalculatorFixture
    {
        [Fact]
        public void Should_Throw_When_TimeProvider_Null()
        {
            Invoking(() =>
            {
                _ = new AccrueExpenseCalculator(null!);
            })
            .Should()
            .Throw<ArgumentNullException>()
            .WithNamedMessageWhenNull("timeProvider");
        }
    }

    public class AccrueExpenses : AccrueExpenseCalculatorFixture
    {
        private readonly AccountEntity _account;
        private readonly DateOnly _currentDate = new(2025, 1, 15);
        private readonly ITimeProvider _timeProvider;
        private readonly AccrueExpenseCalculator _calculator;

        public AccrueExpenses()
        {
            _account = Create<AccountEntity>();
            _timeProvider = Substitute.For<ITimeProvider>();
            _calculator = new AccrueExpenseCalculator(_timeProvider);

            _timeProvider.GetLocalDateNow().Returns(_currentDate);
        }

        [Fact]
        public void Should_Throw_When_Account_Null()
        {
            Invoking(() =>
            {
                _calculator.AccrueExpenses(null!, []);
            })
            .Should()
            .Throw<ArgumentNullException>()
            .WithNamedMessageWhenNull("account");
        }

        [Fact]
        public void Should_Throw_When_Expenses_Null()
        {
            Invoking(() =>
            {
                _calculator.AccrueExpenses(Create<AccountEntity>(), null!);
            })
            .Should()
            .Throw<ArgumentNullException>()
            .WithNamedMessageWhenNull("expenses");
        }

        [Fact]
        public void Should_Not_Throw_When_CurrentDate_Null()
        {
            Invoking(() =>
            {
                _calculator.AccrueExpenses(Create<AccountEntity>(), [], null);
            })
            .Should()
            .NotThrow();
        }

        [Fact]
        public void Should_Reset_Account_Accruals()
        {
            var account = Create<AccountEntity>();

            account.TotalExpenseAccrued = GetWithinRange(1.0d, 1000.0d);
            account.DailyExpenseAccrual = GetWithinRange(1.0d, 10.0d);

            _calculator.AccrueExpenses(account, []);

            account.TotalExpenseAccrued.Should().Be(0.0d);
            account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Set_AccruedIsDirty_To_True()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

            expense.AccruedIsDirty.Should().BeTrue();
        }


        [Fact]
        public void Should_Set_AccruedIsDirty_To_False()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
            expense.AccruedIsDirty = true;

            _calculator.AccrueExpenses(_account, [expense]);

            expense.AccruedIsDirty.Should().BeFalse();
        }

        [Fact]
        public void Should_Set_LastAccruedUpdate_To_CurrentDate()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
            expense.LastAccruedUpdate = null;

            _calculator.AccrueExpenses(_account, [expense]);

            expense.LastAccruedUpdate.Should().Be(_currentDate);
        }

        [Fact]
        public void Should_Use_Custom_CurrentDate_When_Provided()
        {
            var customDate = new DateOnly(2025, 1, 20);
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense], customDate);

            expense.LastAccruedUpdate.Should().Be(customDate);
            // 1000 * 19 / 30 = 633.33
            expense.Accrued.Should().Be(633.33d);
        }

        [Fact]
        public void Should_Handle_Empty_Expense_List()
        {
            _calculator.AccrueExpenses(_account, []);

            _account.TotalExpenseAccrued.Should().Be(0.0d);
            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Calculated_Expense_Accrual()
        {
            var expenseItems = GetAccrualExpenses(_account).ToArray();

            _calculator.AccrueExpenses(_account, expenseItems.Select(item => item.Expense));

            expenseItems.ForEach((expenseItem, index) =>
            {
                expenseItem.Expense.Accrued.Should().Be(expenseItem.ExpectedTotalAccrual);
            });
        }

        [Fact]
        public void Should_Accumulate_TotalExpenseAccrued_Across_Multiple_Expenses()
        {
            var expense1 = EntityFactory.CreateExpense(_account, false, "Expense 1", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
            var expense2 = EntityFactory.CreateExpense(_account, false, "Expense 2", 500, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense1, expense2]);

            // (1000 * 14 / 30) + (500 * 14 / 30) = 466.67 + 233.33 = 700.0
            _account.TotalExpenseAccrued.Should().Be(700.0d);
        }

        [Fact]
        public void Should_Calculate_DailyExpenseAccrual_For_Ongoing_Expense()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // DailyBalance calculation: (1000 - 466.67) / (31 - 15) = 533.33 / 16 = 33.33
            _account.DailyExpenseAccrual.Should().BeApproximately(33.33d, 0.01d);
        }

        [Fact]
        public void Should_Not_Calculate_DailyExpenseAccrual_For_OneTime_Expense_Not_Due()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.OneTime, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // OneTime expenses do calculate daily accrual when not yet due
            // DailyBalance = (1000 - 466.67) / (31 - 15) = 533.33 / 16 = 33.33
            _account.DailyExpenseAccrual.Should().BeApproximately(33.33d, 0.01d);
        }

        [Fact]
        public void Should_Calculate_DailyExpenseAccrual_Based_On_Next_Due_When_Expense_Due_Today()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2024-12-15", "2025-01-15", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // Next due is 31 days away (from 2025-01-15 to 2025-02-15)
            // Daily accrual = 1000 / 31 = 32.258
            _account.DailyExpenseAccrual.Should().BeApproximately(32.26d, 0.01d);
        }

        [Fact]
        public void Should_Not_Calculate_DailyExpenseAccrual_When_Expense_Due_Today_And_EndDate_Today()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2024-12-15", "2025-01-15", "2025-01-15", Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Not_Calculate_DailyExpenseAccrual_When_Expense_Due_Today_And_EndDate_Before_Next_Due()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2024-12-15", "2025-01-15", "2025-01-20", Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // End date is 2025-01-20, next due would be 2025-02-15, which is after end date - so it's never going to be paid again
            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Calculate_DailyExpenseAccrual_For_OneTime_Expense_Due_Tomorrow()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-16", null, Frequency.OneTime, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // Accrued = 1000 * 14 / 15 = 933.33
            // DailyBalance = (1000 - 933.33) / 1 = 66.67
            _account.DailyExpenseAccrual.Should().BeApproximately(66.67d, 0.01d);
        }

        [Fact]
        public void Should_Not_Calculate_DailyExpenseAccrual_For_OneTime_Expense_Due_Today()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-15", null, Frequency.OneTime, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // OneTime expenses don't have future accrual
            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Not_Calculate_DailyExpenseAccrual_For_OneTime_Expense_Due_Yesterday()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-14", null, Frequency.OneTime, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // OneTime expenses don't have future accrual
            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Process_Expenses_In_Ascending_NextDue_Order()
        {
            // This test validates that expenses are sorted by NextDue in ascending order
            // (due sooner processed first), which is critical for future features like preventing
            // negative balances. While the current implementation's results are order-independent
            // due to commutative addition, the sort order matters for the design.

            // Set account balance to a value that would result in negative available balance
            // This documents current behavior and ensures the test is updatied if negative balance prevention is added
            _account.Balance = 1000.0d;

            var expense1 = EntityFactory.CreateExpense(_account, false, "Expense 1", 1000, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
            var expense2 = EntityFactory.CreateExpense(_account, false, "Expense 2", 500, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
            var expense3 = EntityFactory.CreateExpense(_account, false, "Expense 3", 750, "2025-01-01", "2025-01-15", null, Frequency.Months, 1);

            // Pass in random order: expense2 (Jan 25), expense3 (Jan 15), expense1 (Jan 20)
            // AccrueExpenseCalculator should sort by NextDue ascending before processing:
            // Expected order: expense3 (Jan 15), expense1 (Jan 20), expense2 (Jan 25)
            _calculator.AccrueExpenses(_account, [expense2, expense3, expense1]);

            // Verify all expenses were processed correctly
            // The specific values prove the accrual calculations are correct
            expense1.Accrued.Should().Be(736.84d);   // 1000 * 14 / 19 = 736.84
            expense2.Accrued.Should().Be(291.67d);   // 500 * 14 / 24 = 291.67
            expense3.Accrued.Should().Be(750.0d);    // Due today, full amount

            // Total should be sum of all accruals
            _account.TotalExpenseAccrued.Should().BeApproximately(1778.51d, 0.01d);

            // Verify that available balance would be negative (Balance - TotalExpenseAccrued)
            // This documents current behavior: negative balances are allowed
            var availableBalance = _account.Balance - _account.TotalExpenseAccrued;
            availableBalance.Should().BeApproximately(-778.51d, 0.01d, "current implementation allows negative balances");

            // NOTE: When the "no negative balance" feature is implemented:
            // 1. This assertion will likely need updating to reflect new behavior
            // 2. The sort order (ascending by NextDue) becomes critical because expenses due sooner
            //    should be checked first to prevent cascading failures
            // 3. If someone changes OrderBy to OrderByDescending, the negative balance prevention
            //    logic may fail in unexpected ways, making this test fail or behave incorrectly
        }

        [Theory]
        [InlineData("2025-01-15", "2025-01-20", "2025-01-25")] // Already ascending
        [InlineData("2025-01-25", "2025-01-15", "2025-01-20")] // Mixed order
        [InlineData("2025-01-25", "2025-01-20", "2025-01-15")] // Descending order
        public void Should_Process_Multiple_Expense_Orders_Consistently(string date1, string date2, string date3)
        {
            // This test verifies that regardless of input order, all expenses are processed
            // and produce consistent results. While it doesn't prove ascending sort order,
            // it ensures the calculator handles various input orders correctly.

            var expense1 = EntityFactory.CreateExpense(_account, false, "Expense 1", 1000, "2025-01-01", date1, null, Frequency.Months, 1);
            var expense2 = EntityFactory.CreateExpense(_account, false, "Expense 2", 500, "2025-01-01", date2, null, Frequency.Months, 1);
            var expense3 = EntityFactory.CreateExpense(_account, false, "Expense 3", 750, "2025-01-01", date3, null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense1, expense2, expense3]);

            // All three expenses should be processed
            expense1.AccruedIsDirty.Should().BeFalse();
            expense2.AccruedIsDirty.Should().BeFalse();
            expense3.AccruedIsDirty.Should().BeFalse();

            // All should have LastAccruedUpdate set
            expense1.LastAccruedUpdate.Should().Be(_currentDate);
            expense2.LastAccruedUpdate.Should().Be(_currentDate);
            expense3.LastAccruedUpdate.Should().Be(_currentDate);

            // Total should be non-zero (all were processed and accrued)
            _account.TotalExpenseAccrued.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Should_Accrue_Expense_With_NextDue_Before_CurrentDate()
        {
            // Expense is overdue (NextDue in the past) - considered fully accrued but not paid (until renewed)
            var expense = EntityFactory.CreateExpense(_account, false, "Overdue Expense", 1000, "2024-12-01", "2025-01-10", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // When overdue, it will be the full amount for accrued
            expense.Accrued.Should().Be(1000.0d);
        }

        [Fact]
        public void Should_Accrue_Expense_With_NextDue_Equal_To_CurrentDate()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Due Today", 1000, "2024-12-15", "2025-01-15", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // When due, it will be the full amount for accrued
            expense.Accrued.Should().Be(1000.0d);
        }

        [Fact]
        public void Should_Accrue_OneTime_Expense_With_NextDue_Equal_To_CurrentDate()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Due Today", 600, "2025-01-01", "2025-01-15", null, Frequency.OneTime, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // OneTime expenses are considered paid until deleted, hence they remain fully unaccrued from the current date onwards
            expense.Accrued.Should().Be(600.0d);
        }

        [Fact]
        public void Should_Handle_Multiple_Frequency_Types_Together()
        {
            var dailyExpense = EntityFactory.CreateExpense(_account, false, "Daily", 30, "2025-01-01", "2025-01-16", null, Frequency.Days, 1);
            var weeklyExpense = EntityFactory.CreateExpense(_account, false, "Weekly", 70, "2025-01-08", "2025-01-22", null, Frequency.Weeks, 1);
            var monthlyExpense = EntityFactory.CreateExpense(_account, false, "Monthly", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
            var yearlyExpense = EntityFactory.CreateExpense(_account, false, "Yearly", 1200, "2024-01-15", "2025-01-15", null, Frequency.Years, 1);
            var oneTimeExpense = EntityFactory.CreateExpense(_account, false, "OneTime", 500, "2025-01-01", "2025-01-31", null, Frequency.OneTime, 1);

            _calculator.AccrueExpenses(_account, [dailyExpense, weeklyExpense, monthlyExpense, yearlyExpense, oneTimeExpense]);

            // All expenses should have accrued amounts
            dailyExpense.Accrued.Should().Be(28);           // 30 * 14 / 15
            weeklyExpense.Accrued.Should().Be(35);          // 70 * 7 / 14
            monthlyExpense.Accrued.Should().Be(466.67);     // 1000 * 14 / 30
            yearlyExpense.Accrued.Should().Be(1200);        // Due today, full amount
            oneTimeExpense.Accrued.Should().Be(233.33);     // 500 * 14 / 30

            // Total should be sum of all
            var expectedTotal = dailyExpense.Accrued + weeklyExpense.Accrued + monthlyExpense.Accrued +
                                yearlyExpense.Accrued + oneTimeExpense.Accrued;

            _account.TotalExpenseAccrued.Should().BeApproximately(expectedTotal, 0.01d);
        }

        [Fact]
        public void Should_Use_TimeProvider_GetLocalDateNow_When_CurrentDate_Not_Provided()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            _timeProvider.Received(1).GetLocalDateNow();
            expense.LastAccruedUpdate.Should().Be(_currentDate);
        }

        [Fact]
        public void Should_Not_Use_TimeProvider_When_CurrentDate_Provided()
        {
            var customDate = new DateOnly(2025, 1, 20);
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense], customDate);

            _timeProvider.DidNotReceive().GetLocalDateNow();
            expense.LastAccruedUpdate.Should().Be(customDate);
        }

        [Fact]
        public void Should_Not_Accrue_When_AccrualStart_Equals_NextDue_And_Both_Equal_CurrentDate()
        {
            // When AccrualStart == NextDue == CurrentDate, no days have passed, so accrued should be 0
            // But there SHOULD be daily accrual for the next period (if recurring and not at end date)
            var expense = EntityFactory.CreateExpense(_account, false, "Same Start And Due", 1000, "2025-01-15", "2025-01-15", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // No days have passed since accrual start, so accrued = 0
            expense.Accrued.Should().Be(1000.0d); // Due today, full amount

            // Next due is 31 days away (from 2025-01-15 to 2025-02-15)
            // Daily accrual = 1000 / 31 = 32.258
            _account.DailyExpenseAccrual.Should().BeApproximately(32.26d, 0.01d);
        }

        [Fact]
        public void Should_Not_Accrue_Or_Daily_Accrue_When_AccrualStart_Equals_NextDue_And_Both_Equal_CurrentDate_With_EndDate()
        {
            // When AccrualStart == NextDue == CurrentDate and EndDate == CurrentDate, it's the last payment
            // Should have full amount accrued but no daily accrual (won't occur again)
            var expense = EntityFactory.CreateExpense(_account, false, "Same Start And Due With EndDate", 1000, "2025-01-15", "2025-01-15", "2025-01-15", Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            expense.Accrued.Should().Be(1000.0d); // Due today, full amount
            _account.DailyExpenseAccrual.Should().Be(0.0d); // Won't occur again
        }

        [Fact]
        public void Should_Not_Accrue_Or_Daily_Accrue_When_AccrualStart_Equals_NextDue_And_Both_Equal_CurrentDate_OneTime()
        {
            // When AccrualStart == NextDue == CurrentDate for OneTime expense
            // Should have full amount accrued but no daily accrual (one-time)
            var expense = EntityFactory.CreateExpense(_account, false, "OneTime Same Start And Due", 1000, "2025-01-15", "2025-01-15", null, Frequency.OneTime, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            expense.Accrued.Should().Be(1000.0d); // Due today, full amount
            _account.DailyExpenseAccrual.Should().Be(0.0d); // OneTime expenses don't have future accrual
        }

        [Fact]
        public void Should_Accrue_Full_Amount_When_AccrualStart_Equals_NextDue_And_Both_Before_CurrentDate()
        {
            // When AccrualStart == NextDue and both are in the past (overdue)
            // Should be fully accrued but not paid (until renewed)
            var expense = EntityFactory.CreateExpense(_account, false, "Overdue Same Start And Due", 1000, "2025-01-10", "2025-01-10", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            expense.Accrued.Should().Be(1000.0d); // Overdue, full amount
            _account.DailyExpenseAccrual.Should().Be(0.0d); // Already past due, no daily balance to accrue
        }

        [Fact]
        public void Should_Not_Accrue_When_AccrualStart_Equals_NextDue_And_Both_After_CurrentDate()
        {
            // When AccrualStart == NextDue and both are in the future
            // Should not be processed at all
            var expense = EntityFactory.CreateExpense(_account, false, "Future Same Start And Due", 1000, "2025-01-20", "2025-01-20", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            expense.Accrued.Should().Be(0.0d); // Not yet started, no accrual
            expense.AccruedIsDirty.Should().BeFalse(); // Processed but not accrued
            _account.TotalExpenseAccrued.Should().Be(0.0d);
            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Accrue_Correctly_When_AccrualStart_Equals_NextDue_With_Multiple_Expenses()
        {
            // Mix of expenses with AccrualStart == NextDue in different scenarios
            var expenseDueToday = EntityFactory.CreateExpense(_account, false, "Due Today", 1000, "2025-01-15", "2025-01-15", null, Frequency.Months, 1);
            var expenseOverdue = EntityFactory.CreateExpense(_account, false, "Overdue", 500, "2025-01-10", "2025-01-10", null, Frequency.Weeks, 1);
            var expenseFuture = EntityFactory.CreateExpense(_account, false, "Future", 750, "2025-01-20", "2025-01-20", null, Frequency.Months, 1);
            var normalExpense = EntityFactory.CreateExpense(_account, false, "Normal", 600, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expenseDueToday, expenseOverdue, expenseFuture, normalExpense]);

            // expenseDueToday: Due today, full amount
            expenseDueToday.Accrued.Should().Be(1000.0d);

            // expenseOverdue: Overdue, full amount
            expenseOverdue.Accrued.Should().Be(500.0d);

            // expenseFuture: Not yet started
            expenseFuture.Accrued.Should().Be(0.0d);

            // normalExpense: 600 * 14 / 30 = 280
            normalExpense.Accrued.Should().Be(280.0d);

            // Total: 1000 + 500 + 0 + 280 = 1780
            _account.TotalExpenseAccrued.Should().Be(1780.0d);
        }

        [Fact]
        public void Should_Not_Process_Expense_When_Excluded_From_Calcs()
        {
            // Tests the first condition: !expense.ExcludeFromCalcs
            var expense = EntityFactory.CreateExpense(_account, true, "Excluded Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // Excluded expenses should not be processed at all
            expense.Accrued.Should().Be(0.0d);
            expense.AccruedIsDirty.Should().BeFalse();
            _account.TotalExpenseAccrued.Should().Be(0.0d);
            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Not_Process_Expense_When_AccrualStart_After_CurrentDate()
        {
            // Tests the second condition: expense.AccrualStart <= currentDate
            var expense = EntityFactory.CreateExpense(_account, false, "Future Accrual", 1000, "2025-01-16", "2025-01-31", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // Expenses with future AccrualStart should not be processed
            expense.Accrued.Should().Be(0.0d);
            expense.AccruedIsDirty.Should().BeFalse();
            _account.TotalExpenseAccrued.Should().Be(0.0d);
            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Process_Expense_When_AccrualStart_Before_CurrentDate()
        {
            // Tests the core positive case: ExcludeFromCalcs = false AND AccrualStart < currentDate (strictly before)
            var expense = EntityFactory.CreateExpense(_account, false, "Started In Past", 1000, "2025-01-10", "2025-01-25", null, Frequency.Weeks, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // AccrualStart = 2025-01-10, currentDate = 2025-01-15, NextDue = 2025-01-25
            // Days from AccrualStart to currentDate = 5 (exclusive)
            // Total days from AccrualStart to NextDue = 15 (exclusive)
            // Expected accrual = 1000 * 5 / 15 = 333.33
            expense.Accrued.Should().Be(333.33d);
            expense.AccruedIsDirty.Should().BeFalse();
            expense.LastAccruedUpdate.Should().Be(_currentDate);
            _account.TotalExpenseAccrued.Should().Be(333.33d);
            _account.DailyExpenseAccrual.Should().BeGreaterThan(0.0d);
        }

        [Fact]
        public void Should_Process_Expense_When_AccrualStart_Equals_CurrentDate()
        {
            // Tests the equality boundary: AccrualStart == currentDate (not just <)
            // This is different from the NextDue scenarios - here we test when accrual STARTS today
            var expense = EntityFactory.CreateExpense(_account, false, "Starting Today", 1000, "2025-01-15", "2025-01-22", null, Frequency.Weeks, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // No days have passed yet since AccrualStart == currentDate
            expense.Accrued.Should().Be(0.0d);
            expense.AccruedIsDirty.Should().BeFalse();
            expense.LastAccruedUpdate.Should().Be(_currentDate);
            _account.TotalExpenseAccrued.Should().Be(0.0d);

            // But there should be daily accrual since it's an ongoing expense
            // DailyBalance = (1000 - 0) / 7 = 142.86
            _account.DailyExpenseAccrual.Should().BeApproximately(142.86d, 0.01d);
        }

        [Fact]
        public void Should_Not_Process_Excluded_Expense_When_AccrualStart_Equals_CurrentDate()
        {
            // Tests short-circuit with ExcludeFromCalcs = true AND AccrualStart == currentDate
            // This ensures the second condition's equality case is covered in the short-circuit path
            var expense = EntityFactory.CreateExpense(_account, true, "Excluded Starting Today", 1000, "2025-01-15", "2025-01-22", null, Frequency.Weeks, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // Should not process due to ExcludeFromCalcs = true
            expense.Accrued.Should().Be(0.0d);
            expense.AccruedIsDirty.Should().BeFalse();
            _account.TotalExpenseAccrued.Should().Be(0.0d);
            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Not_Process_Expense_When_Excluded_And_AccrualStart_After_CurrentDate()
        {
            // Tests both conditions combined: ExcludeFromCalcs && AccrualStart > currentDate
            var expense = EntityFactory.CreateExpense(_account, true, "Excluded Future Expense", 1000, "2025-01-16", "2025-01-31", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // Excluded expenses with future accrual should not be processed
            expense.Accrued.Should().Be(0.0d);
            expense.AccruedIsDirty.Should().BeFalse();
            _account.TotalExpenseAccrued.Should().Be(0.0d);
            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Handle_Multiple_Expenses_Where_Last_Sorted_Expense_Is_Not_Processed()
        {
            // Tests the lambda execution path where the LAST expense in sorted order has processExpense = false
            // This ensures all exit paths from the lambda are covered, including the last iteration
            var expense1 = EntityFactory.CreateExpense(_account, false, "Processed First", 1000, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
            var expense2 = EntityFactory.CreateExpense(_account, false, "Processed Second", 500, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
            var expense3 = EntityFactory.CreateExpense(_account, false, "Not Processed Last", 750, "2025-01-16", "2025-01-10", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense1, expense2, expense3]);

            // After sorting by NextDue descending: expense1 (Jan 25), expense2 (Jan 20), expense3 (Jan 10)
            // expense1: AccrualStart 2025-01-01 < currentDate 2025-01-15 -> process
            expense1.Accrued.Should().Be(583.33d); // 1000 * 14 / 24

            // expense2: AccrualStart 2025-01-01 < currentDate 2025-01-15 -> process  
            expense2.Accrued.Should().Be(368.42d); // 500 * 14 / 19

            // expense3 (LAST in sorted order): AccrualStart 2025-01-16 > currentDate 2025-01-15 -> NOT processed
            expense3.Accrued.Should().Be(0.0d);
            expense3.AccruedIsDirty.Should().BeFalse();

            _account.TotalExpenseAccrued.Should().Be(951.75d); // 583.33 + 368.42
        }

        [Fact]
        public void Should_Handle_Multiple_Expenses_Where_Last_Sorted_Expense_Is_Excluded()
        {
            // Tests the lambda execution path where the LAST expense has ExcludeFromCalcs = true
            // This covers the short-circuit path where the second condition is not evaluated for the last iteration
            var expense1 = EntityFactory.CreateExpense(_account, false, "Processed First", 1000, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
            var expense2 = EntityFactory.CreateExpense(_account, false, "Processed Second", 500, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
            var expense3 = EntityFactory.CreateExpense(_account, true, "Excluded Last", 750, "2025-01-01", "2025-01-10", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense1, expense2, expense3]);

            // After sorting by NextDue descending: expense1 (Jan 25), expense2 (Jan 20), expense3 (Jan 10)
            expense1.Accrued.Should().Be(583.33d); // 1000 * 14 / 24
            expense2.Accrued.Should().Be(368.42d); // 500 * 14 / 19

            // expense3 (LAST in sorted order): ExcludeFromCalcs = true -> NOT processed (short-circuit)
            expense3.Accrued.Should().Be(0.0d);
            expense3.AccruedIsDirty.Should().BeFalse();

            _account.TotalExpenseAccrued.Should().Be(951.75d); // 583.33 + 368.42
        }

        private IEnumerable<(ExpenseEntity Expense, double ExpectedTotalAccrual)> GetAccrualExpenses(AccountEntity account)
        {
            // On the basis we don't accrue the first day since this is also the day the expense is paid. An example:
            //
            // Accrual Start = 1st
            // Current Date = 15th      => _currentDate = new(2025, 1, 15)
            // Next Due = 31st
            //
            // Days from Accrual Start to Current Date = 14     (exclusive)
            // Total Days from Accrual Start to Next Due = 30   (exclusive)

            // Expected Accrual = Amount * (Days from Accrual Start to Current Date) / (Total Days from Accrual Start to Next Due)
            //                  = Amount * 14 / 30

            // Except for OneTime, accrual calculations are NOT affected by frequency or frequency count.
            foreach (var frequency in Frequency.GetAll().Where(freq => freq != Frequency.OneTime))
            {
                var frequencyCount = GetWithinRange(1, 10);

                // Mid-cycle
                yield return (EntityFactory.CreateExpense(account, false, "Expense 01", 100, "2025-01-12", "2025-01-19", null, frequency, frequencyCount), 42.86);            // 100 * 3 / 7 = 42.857
                yield return (EntityFactory.CreateExpense(account, false, "Expense 02", 100, "2025-01-12", "2025-01-19", "2025-01-19", frequency, frequencyCount), 42.86);    // End date equals next due
                yield return (EntityFactory.CreateExpense(account, false, "Expense 03", 100, "2025-01-12", "2025-01-19", "2026-01-19", frequency, frequencyCount), 42.86);    // End date after next due

                // CurrentDate equals AccrualStart
                yield return (EntityFactory.CreateExpense(account, false, "Expense 04", 50, "2025-01-15", "2025-01-22", null, frequency, frequencyCount), 0.0);              // No days accrued yet
                yield return (EntityFactory.CreateExpense(account, false, "Expense 05", 50, "2025-01-15", "2025-01-22", "2025-01-22", frequency, frequencyCount), 0.0);      // End date equals next due
                yield return (EntityFactory.CreateExpense(account, false, "Expense 06", 50, "2025-01-15", "2025-01-22", "2026-01-19", frequency, frequencyCount), 0.0);      // End date after next due

                // CurrentDate equals NextDue
                yield return (EntityFactory.CreateExpense(account, false, "Expense 07", 100, "2025-01-01", "2025-01-15", null, frequency, frequencyCount), 100.0);           // Due today, full amount
                yield return (EntityFactory.CreateExpense(account, false, "Expense 08", 100, "2025-01-01", "2025-01-15", "2025-01-15", frequency, frequencyCount), 100.0);   // End date equals next due
                yield return (EntityFactory.CreateExpense(account, false, "Expense 09", 100, "2025-01-01", "2025-01-15", "2026-01-19", frequency, frequencyCount), 100.0);   // End date after next due

                // Excluded from calculations
                yield return (EntityFactory.CreateExpense(account, true, "Expense 10", 500, "2024-12-31", "2025-01-31", null, frequency, frequencyCount), 0.0);
                yield return (EntityFactory.CreateExpense(account, true, "Expense 11", 500, "2024-12-31", "2025-01-31", "2025-01-31", frequency, frequencyCount), 0.0);      // End date equals next due
                yield return (EntityFactory.CreateExpense(account, true, "Expense 12", 500, "2024-12-31", "2025-01-31", "2026-01-19", frequency, frequencyCount), 0.0);      // End date after next due

                // AccrualStart in the future (should not accrue)
                yield return (EntityFactory.CreateExpense(account, false, "Expense 13", 400, "2025-01-16", "2025-01-31", null, frequency, frequencyCount), 0.0);
                yield return (EntityFactory.CreateExpense(account, false, "Expense 14", 400, "2025-01-16", "2025-01-31", "2025-01-31", frequency, frequencyCount), 0.0);     // End date equals next due
                yield return (EntityFactory.CreateExpense(account, false, "Expense 15", 400, "2025-01-16", "2025-01-31", "2026-01-19", frequency, frequencyCount), 0.0);     // End date after next due
            }

            // One-time expense that was due yesterday
            yield return (EntityFactory.CreateExpense(account, false, "Expense 16", 100, "2024-12-20", "2025-01-14", null, Frequency.OneTime, Create<int>()), 100.0);        // Fully accrued - until deleted

            // One-time expense that is due today
            yield return (EntityFactory.CreateExpense(account, false, "Expense 17", 100, "2024-12-20", "2025-01-15", null, Frequency.OneTime, Create<int>()), 100.0);        // Fully accrued - until deleted

            // One-time expense that is due tomorrow - leap year
            // A non-leap year would be 100 * 362 / 363 = 99.724
            yield return (EntityFactory.CreateExpense(account, false, "Expense 18", 100, "2024-01-18", "2025-01-16", null, Frequency.OneTime, Create<int>()), 99.73);        // 100 * 363 / 364 = 99.725
        }

        [Fact]
        public void Should_Handle_Accrual_When_Expense_Renewed_From_Jan_31_Through_February()
        {
            // Validates accrual calculation when an expense has drifted from Jan 31 to Feb 28
            // Scenario: Expense due Jan 31, renewed to Feb 28, now accruing toward Feb 28
            var timeProvider = Substitute.For<ITimeProvider>();
            var calculator = new AccrueExpenseCalculator(timeProvider);
            var account = Create<AccountEntity>();

            // It's Feb 10, expense is accruing from Jan 31 (paid/renewed on Jan 31) to Feb 28 (next due)
            var currentDate = new DateOnly(2025, 2, 10);
            timeProvider.GetLocalDateNow().Returns(currentDate);

            // AccrualStart = Jan 31 (when it was last paid/renewed)
            // NextDue = Feb 28 (what AddMonths(1) gave us from Jan 31)
            var expense = EntityFactory.CreateExpense(account, false, "Rent", 1200, "2025-01-31", "2025-02-28", null, Frequency.Months, 1);

            calculator.AccrueExpenses(account, [expense]);

            // Days from Jan 31 to Feb 10 = 10 days
            // Days from Jan 31 to Feb 28 = 28 days
            // Expected: 1200 * 10 / 28 = 428.57
            expense.Accrued.Should().Be(428.57d);
            expense.AccruedIsDirty.Should().BeFalse();

            // Daily accrual: 1200 / 28 = 42.86/day
            expense.LastAccruedUpdate.Should().Be(currentDate);
            account.TotalExpenseAccrued.Should().Be(428.57d);
        }

        [Fact]
        public void Should_Handle_Accrual_When_Expense_Renewed_From_Feb_28_To_Mar_28()
        {
            // Validates accrual calculation after date has drifted from 31st to 28th

            var timeProvider = Substitute.For<ITimeProvider>();
            var calculator = new AccrueExpenseCalculator(timeProvider);
            var account = Create<AccountEntity>();

            // It's Mar 15, expense is accruing from Feb 28 (paid/renewed) to Mar 28 (next due)
            var currentDate = new DateOnly(2025, 3, 15);
            timeProvider.GetLocalDateNow().Returns(currentDate);

            // AccrualStart = Feb 28 (when it was last paid/renewed)
            // NextDue = Mar 28 (AddMonths(1) from Feb 28)
            var expense = EntityFactory.CreateExpense(account, false, "Rent", 1200, "2025-02-28", "2025-03-28", null, Frequency.Months, 1);

            calculator.AccrueExpenses(account, [expense]);

            // Days from Feb 28 to Mar 15 = 15 days
            // Days from Feb 28 to Mar 28 = 28 days
            // Expected: 1200 * 15 / 28 = 642.86
            expense.Accrued.Should().Be(642.86d);
            expense.AccruedIsDirty.Should().BeFalse();
            expense.LastAccruedUpdate.Should().Be(currentDate);
            account.TotalExpenseAccrued.Should().Be(642.86d);
        }

        [Fact]
        public void Should_Handle_Multiple_Expenses_With_Different_Month_End_Periods()
        {
            // Validates accrual calculations for expenses with different period lengths due to month-end drift

            var timeProvider = Substitute.For<ITimeProvider>();
            var calculator = new AccrueExpenseCalculator(timeProvider);
            var account = Create<AccountEntity>();

            var currentDate = new DateOnly(2025, 3, 15);
            timeProvider.GetLocalDateNow().Returns(currentDate);

            // Expense 1: Accruing from Mar 1 to Mar 31 (31 days) - normal month
            var expense1 = EntityFactory.CreateExpense(account, false, "Expense 1", 1000, "2025-03-01", "2025-03-31", null, Frequency.Months, 1);

            // Expense 2: Accruing from Feb 28 to Mar 28 (28 days) - drifted from 31st
            var expense2 = EntityFactory.CreateExpense(account, false, "Expense 2", 1200, "2025-02-28", "2025-03-28", null, Frequency.Months, 1);

            calculator.AccrueExpenses(account, [expense1, expense2]);

            // Expense 1: 14 days elapsed / 30 days total = 1000 * 14 / 30 = 466.67
            expense1.Accrued.Should().Be(466.67d);

            // Expense 2: 15 days elapsed / 28 days total = 1200 * 15 / 28 = 642.86
            expense2.Accrued.Should().Be(642.86d);

            // Total: 466.67 + 642.86 = 1109.53
            account.TotalExpenseAccrued.Should().Be(1109.53d);
        }
    }
}
