using AllOverIt.Extensions;
using AllOverIt.Fixture.Extensions;
using FluentAssertions;
using NSubstitute;
using Pot.App.Calculators;
using Pot.App.Concerns.Time;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;

namespace Pot.App.Tests.Calculators;

public class AccrueExpenseCalculatorFixture : CalculatorFixtureBase
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
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

            expense.AccruedIsDirty.Should().BeTrue();
        }


        [Fact]
        public void Should_Set_AccruedIsDirty_To_False()
        {
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
            expense.AccruedIsDirty = true;

            _calculator.AccrueExpenses(_account, [expense]);

            expense.AccruedIsDirty.Should().BeFalse();
        }

        [Fact]
        public void Should_Set_LastAccruedUpdate_To_CurrentDate()
        {
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
            expense.LastAccruedUpdate = null;

            _calculator.AccrueExpenses(_account, [expense]);

            expense.LastAccruedUpdate.Should().Be(_currentDate);
        }

        [Fact]
        public void Should_Use_Custom_CurrentDate_When_Provided()
        {
            var customDate = new DateOnly(2025, 1, 20);
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

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
            var expense1 = CreateExpense(_account, false, "Expense 1", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
            var expense2 = CreateExpense(_account, false, "Expense 2", 500, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense1, expense2]);

            // (1000 * 14 / 30) + (500 * 14 / 30) = 466.67 + 233.33 = 700.0
            _account.TotalExpenseAccrued.Should().Be(700.0d);
        }

        [Fact]
        public void Should_Calculate_DailyExpenseAccrual_For_Ongoing_Expense()
        {
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // DailyBalance calculation: (1000 - 466.67) / (31 - 15) = 533.33 / 16 = 33.33
            _account.DailyExpenseAccrual.Should().BeApproximately(33.33d, 0.01d);
        }

        [Fact]
        public void Should_Not_Calculate_DailyExpenseAccrual_For_OneTime_Expense_Not_Due()
        {
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.OneTime, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // OneTime expenses do calculate daily accrual when not yet due
            // DailyBalance = (1000 - 466.67) / (31 - 15) = 533.33 / 16 = 33.33
            _account.DailyExpenseAccrual.Should().BeApproximately(33.33d, 0.01d);
        }

        [Fact]
        public void Should_Calculate_DailyExpenseAccrual_Based_On_Next_Due_When_Expense_Due_Today()
        {
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2024-12-15", "2025-01-15", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // Next due is 31 days away (from 2025-01-15 to 2025-02-15)
            // Daily accrual = 1000 / 31 = 32.258
            _account.DailyExpenseAccrual.Should().BeApproximately(32.26d, 0.01d);
        }

        [Fact]
        public void Should_Not_Calculate_DailyExpenseAccrual_When_Expense_Due_Today_And_EndDate_Today()
        {
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2024-12-15", "2025-01-15", "2025-01-15", Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Not_Calculate_DailyExpenseAccrual_When_Expense_Due_Today_And_EndDate_Before_Next_Due()
        {
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2024-12-15", "2025-01-15", "2025-01-20", Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // End date is 2025-01-20, next due would be 2025-02-15, which is after end date - so it's never going to be paid again
            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Calculate_DailyExpenseAccrual_For_OneTime_Expense_Due_Tomorrow()
        {
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-16", null, Frequency.OneTime, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // Accrued = 1000 * 14 / 15 = 933.33
            // DailyBalance = (1000 - 933.33) / 1 = 66.67
            _account.DailyExpenseAccrual.Should().BeApproximately(66.67d, 0.01d);
        }

        [Fact]
        public void Should_Not_Calculate_DailyExpenseAccrual_For_OneTime_Expense_Due_Today()
        {
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-15", null, Frequency.OneTime, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // OneTime expenses don't have future accrual
            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Not_Calculate_DailyExpenseAccrual_For_OneTime_Expense_Due_Yesterday()
        {
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-14", null, Frequency.OneTime, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // OneTime expenses don't have future accrual
            _account.DailyExpenseAccrual.Should().Be(0.0d);
        }

        [Fact]
        public void Should_Process_Expenses_In_Descending_NextDue_Order()
        {
            var expense1 = CreateExpense(_account, false, "Expense 1", 1000, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
            var expense2 = CreateExpense(_account, false, "Expense 2", 500, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
            var expense3 = CreateExpense(_account, false, "Expense 3", 750, "2025-01-01", "2025-01-15", null, Frequency.Months, 1);

            // Pass in random order
            _calculator.AccrueExpenses(_account, [expense2, expense3, expense1]);

            // All should be processed regardless of order
            expense1.Accrued.Should().Be(736.84d);   // 1000 * 14 / 19 = 736.84
            expense2.Accrued.Should().Be(291.67d);   // 500 * 14 / 24 = 291.67
            expense3.Accrued.Should().Be(750.0d);    // Due today, full amount
        }

        [Fact]
        public void Should_Accrue_Expense_With_NextDue_Before_CurrentDate()
        {
            // Expense is overdue (NextDue in the past) - considered fully accrued but not paid (until renewed)
            var expense = CreateExpense(_account, false, "Overdue Expense", 1000, "2024-12-01", "2025-01-10", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // When overdue, it will be the full amount for accrued
            expense.Accrued.Should().Be(1000.0d);
        }

        [Fact]
        public void Should_Accrue_Expense_With_NextDue_Equal_To_CurrentDate()
        {
            var expense = CreateExpense(_account, false, "Due Today", 1000, "2024-12-15", "2025-01-15", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // When due, it will be the full amount for accrued
            expense.Accrued.Should().Be(1000.0d);
        }

        [Fact]
        public void Should_Accrue_OneTime_Expense_With_NextDue_Equal_To_CurrentDate()
        {
            var expense = CreateExpense(_account, false, "Due Today", 600, "2025-01-01", "2025-01-15", null, Frequency.OneTime, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            // OneTime expenses are considered paid until deleted, hence they remain fully unaccrued from the current date onwards
            expense.Accrued.Should().Be(600.0d);
        }

        [Fact]
        public void Should_Handle_Multiple_Frequency_Types_Together()
        {
            var dailyExpense = CreateExpense(_account, false, "Daily", 30, "2025-01-01", "2025-01-16", null, Frequency.Days, 1);
            var weeklyExpense = CreateExpense(_account, false, "Weekly", 70, "2025-01-08", "2025-01-22", null, Frequency.Weeks, 1);
            var monthlyExpense = CreateExpense(_account, false, "Monthly", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);
            var yearlyExpense = CreateExpense(_account, false, "Yearly", 1200, "2024-01-15", "2025-01-15", null, Frequency.Years, 1);
            var oneTimeExpense = CreateExpense(_account, false, "OneTime", 500, "2025-01-01", "2025-01-31", null, Frequency.OneTime, 1);

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
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            _timeProvider.Received(1).GetLocalDateNow();
            expense.LastAccruedUpdate.Should().Be(_currentDate);
        }

        [Fact]
        public void Should_Not_Use_TimeProvider_When_CurrentDate_Provided()
        {
            var customDate = new DateOnly(2025, 1, 20);
            var expense = CreateExpense(_account, false, "Test Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense], customDate);

            _timeProvider.DidNotReceive().GetLocalDateNow();
            expense.LastAccruedUpdate.Should().Be(customDate);
        }

        [Fact]
        public void Should_Not_Accrue_When_AccrualStart_Equals_NextDue_And_Both_Equal_CurrentDate()
        {
            // When AccrualStart == NextDue == CurrentDate, no days have passed, so accrued should be 0
            // But there SHOULD be daily accrual for the next period (if recurring and not at end date)
            var expense = CreateExpense(_account, false, "Same Start And Due", 1000, "2025-01-15", "2025-01-15", null, Frequency.Months, 1);

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
            var expense = CreateExpense(_account, false, "Same Start And Due With EndDate", 1000, "2025-01-15", "2025-01-15", "2025-01-15", Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            expense.Accrued.Should().Be(1000.0d); // Due today, full amount
            _account.DailyExpenseAccrual.Should().Be(0.0d); // Won't occur again
        }

        [Fact]
        public void Should_Not_Accrue_Or_Daily_Accrue_When_AccrualStart_Equals_NextDue_And_Both_Equal_CurrentDate_OneTime()
        {
            // When AccrualStart == NextDue == CurrentDate for OneTime expense
            // Should have full amount accrued but no daily accrual (one-time)
            var expense = CreateExpense(_account, false, "OneTime Same Start And Due", 1000, "2025-01-15", "2025-01-15", null, Frequency.OneTime, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            expense.Accrued.Should().Be(1000.0d); // Due today, full amount
            _account.DailyExpenseAccrual.Should().Be(0.0d); // OneTime expenses don't have future accrual
        }

        [Fact]
        public void Should_Accrue_Full_Amount_When_AccrualStart_Equals_NextDue_And_Both_Before_CurrentDate()
        {
            // When AccrualStart == NextDue and both are in the past (overdue)
            // Should be fully accrued but not paid (until renewed)
            var expense = CreateExpense(_account, false, "Overdue Same Start And Due", 1000, "2025-01-10", "2025-01-10", null, Frequency.Months, 1);

            _calculator.AccrueExpenses(_account, [expense]);

            expense.Accrued.Should().Be(1000.0d); // Overdue, full amount
            _account.DailyExpenseAccrual.Should().Be(0.0d); // Already past due, no daily balance to accrue
        }

        [Fact]
        public void Should_Not_Accrue_When_AccrualStart_Equals_NextDue_And_Both_After_CurrentDate()
        {
            // When AccrualStart == NextDue and both are in the future
            // Should not be processed at all
            var expense = CreateExpense(_account, false, "Future Same Start And Due", 1000, "2025-01-20", "2025-01-20", null, Frequency.Months, 1);

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
            var expenseDueToday = CreateExpense(_account, false, "Due Today", 1000, "2025-01-15", "2025-01-15", null, Frequency.Months, 1);
            var expenseOverdue = CreateExpense(_account, false, "Overdue", 500, "2025-01-10", "2025-01-10", null, Frequency.Weeks, 1);
            var expenseFuture = CreateExpense(_account, false, "Future", 750, "2025-01-20", "2025-01-20", null, Frequency.Months, 1);
            var normalExpense = CreateExpense(_account, false, "Normal", 600, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

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
            var expense = CreateExpense(_account, true, "Excluded Expense", 1000, "2025-01-01", "2025-01-31", null, Frequency.Months, 1);

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
            var expense = CreateExpense(_account, false, "Future Accrual", 1000, "2025-01-16", "2025-01-31", null, Frequency.Months, 1);

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
            var expense = CreateExpense(_account, false, "Started In Past", 1000, "2025-01-10", "2025-01-25", null, Frequency.Weeks, 1);

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
            var expense = CreateExpense(_account, false, "Starting Today", 1000, "2025-01-15", "2025-01-22", null, Frequency.Weeks, 1);

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
            var expense = CreateExpense(_account, true, "Excluded Starting Today", 1000, "2025-01-15", "2025-01-22", null, Frequency.Weeks, 1);

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
            var expense = CreateExpense(_account, true, "Excluded Future Expense", 1000, "2025-01-16", "2025-01-31", null, Frequency.Months, 1);

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
            var expense1 = CreateExpense(_account, false, "Processed First", 1000, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
            var expense2 = CreateExpense(_account, false, "Processed Second", 500, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
            var expense3 = CreateExpense(_account, false, "Not Processed Last", 750, "2025-01-16", "2025-01-10", null, Frequency.Months, 1);

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
            var expense1 = CreateExpense(_account, false, "Processed First", 1000, "2025-01-01", "2025-01-25", null, Frequency.Months, 1);
            var expense2 = CreateExpense(_account, false, "Processed Second", 500, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
            var expense3 = CreateExpense(_account, true, "Excluded Last", 750, "2025-01-01", "2025-01-10", null, Frequency.Months, 1);

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
                yield return (CreateExpense(account, false, "Expense 01", 100, "2025-01-12", "2025-01-19", null, frequency, frequencyCount), 42.86);            // 100 * 3 / 7 = 42.857
                yield return (CreateExpense(account, false, "Expense 02", 100, "2025-01-12", "2025-01-19", "2025-01-19", frequency, frequencyCount), 42.86);    // End date equals next due
                yield return (CreateExpense(account, false, "Expense 03", 100, "2025-01-12", "2025-01-19", "2026-01-19", frequency, frequencyCount), 42.86);    // End date after next due

                // CurrentDate equals AccrualStart
                yield return (CreateExpense(account, false, "Expense 04", 50, "2025-01-15", "2025-01-22", null, frequency, frequencyCount), 0.0);              // No days accrued yet
                yield return (CreateExpense(account, false, "Expense 05", 50, "2025-01-15", "2025-01-22", "2025-01-22", frequency, frequencyCount), 0.0);      // End date equals next due
                yield return (CreateExpense(account, false, "Expense 06", 50, "2025-01-15", "2025-01-22", "2026-01-19", frequency, frequencyCount), 0.0);      // End date after next due

                // CurrentDate equals NextDue
                yield return (CreateExpense(account, false, "Expense 07", 100, "2025-01-01", "2025-01-15", null, frequency, frequencyCount), 100.0);           // Due today, full amount
                yield return (CreateExpense(account, false, "Expense 08", 100, "2025-01-01", "2025-01-15", "2025-01-15", frequency, frequencyCount), 100.0);   // End date equals next due
                yield return (CreateExpense(account, false, "Expense 09", 100, "2025-01-01", "2025-01-15", "2026-01-19", frequency, frequencyCount), 100.0);   // End date after next due

                // Excluded from calculations
                yield return (CreateExpense(account, true, "Expense 10", 500, "2024-12-31", "2025-01-31", null, frequency, frequencyCount), 0.0);
                yield return (CreateExpense(account, true, "Expense 11", 500, "2024-12-31", "2025-01-31", "2025-01-31", frequency, frequencyCount), 0.0);      // End date equals next due
                yield return (CreateExpense(account, true, "Expense 12", 500, "2024-12-31", "2025-01-31", "2026-01-19", frequency, frequencyCount), 0.0);      // End date after next due

                // AccrualStart in the future (should not accrue)
                yield return (CreateExpense(account, false, "Expense 13", 400, "2025-01-16", "2025-01-31", null, frequency, frequencyCount), 0.0);
                yield return (CreateExpense(account, false, "Expense 14", 400, "2025-01-16", "2025-01-31", "2025-01-31", frequency, frequencyCount), 0.0);     // End date equals next due
                yield return (CreateExpense(account, false, "Expense 15", 400, "2025-01-16", "2025-01-31", "2026-01-19", frequency, frequencyCount), 0.0);     // End date after next due
            }

            // One-time expense that was due yesterday
            yield return (CreateExpense(account, false, "Expense 16", 100, "2024-12-20", "2025-01-14", null, Frequency.OneTime, Create<int>()), 100.0);        // Fully accrued - until deleted

            // One-time expense that is due today
            yield return (CreateExpense(account, false, "Expense 17", 100, "2024-12-20", "2025-01-15", null, Frequency.OneTime, Create<int>()), 100.0);        // Fully accrued - until deleted

            // One-time expense that is due tomorrow - leap year
            // A non-leap year would be 100 * 362 / 363 = 99.724
            yield return (CreateExpense(account, false, "Expense 18", 100, "2024-01-18", "2025-01-16", null, Frequency.OneTime, Create<int>()), 99.73);        // 100 * 363 / 364 = 99.725
        }
    }
}
