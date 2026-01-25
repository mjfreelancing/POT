using AllOverIt.Fixture.Extensions;
using FluentAssertions;
using Pot.App.Calculators;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.TestUtils;

namespace Pot.App.Tests.Calculators;

public class ExpenseRenewalCalculatorFixture : PotFixtureBase
{
    public ExpenseRenewalCalculatorFixture()
    {
        CustomizeEnumerations();
        OmitRecursionBehavior();
    }

    public class Renew : ExpenseRenewalCalculatorFixture
    {
        private readonly AccountEntity _account;
        private readonly ExpenseRenewalCalculator _calculator;

        public Renew()
        {
            _account = Create<AccountEntity>();
            _calculator = new ExpenseRenewalCalculator();
        }

        [Fact]
        public void Should_Throw_When_Expenses_Null()
        {
            Invoking(() =>
            {
                _calculator.Renew(null!, DateOnly.MinValue);
            })
            .Should()
            .Throw<ArgumentNullException>()
            .WithNamedMessageWhenNull("expenses");
        }

        [Fact]
        public void Should_Handle_Empty_Expense_List()
        {
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            Invoking(() =>
            {
                _calculator.Renew([], advanceUntilDate);
            })
            .Should()
            .NotThrow();
        }

        [Fact]
        public void Should_Not_Renew_Expense_Excluded_From_Calcs()
        {
            var expense = EntityFactory.CreateExpense(_account, true, "Excluded Expense", 100, "2025-01-01", "2025-01-15", null, Frequency.Months, 1);
            expense.AccruedIsDirty = false;

            var advanceUntilDate = new DateOnly(2025, 1, 20);

            var originalNextDue = expense.NextDue;
            var originalAccrualStart = expense.AccrualStart;

            _calculator.Renew([expense], advanceUntilDate);

            expense.NextDue.Should().Be(originalNextDue);
            expense.AccrualStart.Should().Be(originalAccrualStart);
            expense.AccruedIsDirty.Should().BeFalse();
        }

        [Fact]
        public void Should_Not_Renew_One_Time_Expense()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "OneTime Expense", 100, "2025-01-01", "2025-01-15", null, Frequency.OneTime, 1);
            expense.AccruedIsDirty = false;

            var advanceUntilDate = new DateOnly(2025, 1, 20);

            var originalNextDue = expense.NextDue;
            var originalAccrualStart = expense.AccrualStart;

            _calculator.Renew([expense], advanceUntilDate);

            expense.NextDue.Should().Be(originalNextDue);
            expense.AccrualStart.Should().Be(originalAccrualStart);
            expense.AccruedIsDirty.Should().BeFalse();
        }

        [Fact]
        public void Should_Not_Reset_IsDirty_When_Expense_Not_Renewed()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Future Expense", 100, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
            expense.AccruedIsDirty = true;

            var advanceUntilDate = new DateOnly(2025, 1, 15);

            _calculator.Renew([expense], advanceUntilDate);

            // Expense should not be renewed because NextDue > advanceUntilDate
            expense.AccruedIsDirty.Should().BeTrue();
        }

        [Fact]
        public void Should_Set_IsDirty_When_Expense_Is_Renewed()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Past Due Expense", 100, "2025-01-01", "2025-01-10", null, Frequency.Months, 1);
            expense.AccruedIsDirty = false;

            var advanceUntilDate = new DateOnly(2025, 1, 20);

            _calculator.Renew([expense], advanceUntilDate);

            // Expense should be renewed, and IsDirty should become true
            expense.AccruedIsDirty.Should().BeTrue();
        }

        [Fact]
        public void Should_Not_Set_IsDirty_When_Expense_Is_Not_Renewed()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Future Expense", 100, "2025-01-01", "2025-01-20", null, Frequency.Months, 1);
            expense.AccruedIsDirty = false;

            var advanceUntilDate = new DateOnly(2025, 1, 15);

            _calculator.Renew([expense], advanceUntilDate);

            // Expense should not be renewed because NextDue > advanceUntilDate
            expense.AccruedIsDirty.Should().BeFalse();
        }

        [Fact]
        public void Should_Not_Renew_Expense_With_EndDate_Before_AdvanceUntilDate()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Ending Expense", 100, "2025-01-01", "2025-01-10", "2025-01-15", Frequency.Months, 1);
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            var originalNextDue = expense.NextDue;
            var originalAccrualStart = expense.AccrualStart;

            _calculator.Renew([expense], advanceUntilDate);

            // Expense should not renew because advanceUntilDate >= endDate
            expense.NextDue.Should().Be(originalNextDue);
            expense.AccrualStart.Should().Be(originalAccrualStart);
        }

        [Fact]
        public void Should_Not_Renew_Expense_With_NextDue_Date_After_EndDate()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Limited Expense", 100, "2025-01-01", "2025-01-10", "2025-01-20", Frequency.Months, 1);
            var advanceUntilDate = new DateOnly(2025, 1, 15);

            _calculator.Renew([expense], advanceUntilDate);

            // Expense should be renewed to 2025-02-10, but since that's after EndDate (2025-01-20), it should stop at 2025-01-10
            expense.NextDue.Should().Be(new DateOnly(2025, 1, 10));
            expense.AccrualStart.Should().Be(new DateOnly(2025, 1, 1));
        }

        [Fact]
        public void Should_Renew_Expenses_With_Daily_Frequency()
        {
            // Includes multiple renewals where:
            // 1 expense is 3 days past due
            // 1 expense is 1 day past due
            // 1 expense is due today
            // 1 expense is not yet due

            var advanceUntilDate = new DateOnly(2025, 1, 20);

            // 3 days past due - should advance 3 times
            var expense1 = EntityFactory.CreateExpense(_account, false, "3 Days Past", 100, "2025-01-10", "2025-01-17", null, Frequency.Days, 1);
            expense1.AccruedIsDirty = false;

            // 1 day past due - should advance 1 time
            var expense2 = EntityFactory.CreateExpense(_account, false, "1 Day Past", 100, "2025-01-12", "2025-01-19", null, Frequency.Days, 1);
            expense2.AccruedIsDirty = false;

            // Due today - should not advance (only advance if < advanceUntilDate)
            var expense3 = EntityFactory.CreateExpense(_account, false, "Due Today", 100, "2025-01-13", "2025-01-20", null, Frequency.Days, 1);
            expense3.AccruedIsDirty = false;

            // Not yet due - should not advance
            var expense4 = EntityFactory.CreateExpense(_account, false, "Future Due", 100, "2025-01-14", "2025-01-21", null, Frequency.Days, 1);
            expense4.AccruedIsDirty = false;

            _calculator.Renew([expense1, expense2, expense3, expense4], advanceUntilDate);

            // expense1: 2025-01-17 -> 2025-01-18 -> 2025-01-19 -> 2025-01-20 -> 2025-01-21
            expense1.NextDue.Should().Be(new DateOnly(2025, 1, 21));
            expense1.AccrualStart.Should().Be(new DateOnly(2025, 1, 20));
            expense1.AccruedIsDirty.Should().BeTrue();

            // expense2: 2025-01-19 -> 2025-01-20 -> 2025-01-21
            expense2.NextDue.Should().Be(new DateOnly(2025, 1, 21));
            expense2.AccrualStart.Should().Be(new DateOnly(2025, 1, 20));
            expense2.AccruedIsDirty.Should().BeTrue();

            // expense3: 2025-01-20 -> 2025-01-21 (does renew once since nextDue <= advanceUntilDate)
            expense3.NextDue.Should().Be(new DateOnly(2025, 1, 21));
            expense3.AccrualStart.Should().Be(new DateOnly(2025, 1, 20));
            expense3.AccruedIsDirty.Should().BeTrue();

            // expense4: No change (NextDue > advanceUntilDate)
            expense4.NextDue.Should().Be(new DateOnly(2025, 1, 21));
            expense4.AccrualStart.Should().Be(new DateOnly(2025, 1, 14));
            expense4.AccruedIsDirty.Should().BeFalse();
        }

        [Fact]
        public void Should_Renew_Expenses_With_Weekly_Frequency()
        {
            // Includes multiple renewals where:
            // 1 expense is 3 weeks past due
            // 1 expense is 1 week past due
            // 1 expense is due today
            // 1 expense is not yet due

            var advanceUntilDate = new DateOnly(2025, 2, 10);

            // 3 weeks past due - should advance 3 times (7 days each)
            var expense1 = EntityFactory.CreateExpense(_account, false, "3 Weeks Past", 100, "2025-01-06", "2025-01-20", null, Frequency.Weeks, 1);
            expense1.AccruedIsDirty = false;

            // 1 week past due - should advance 1 time
            var expense2 = EntityFactory.CreateExpense(_account, false, "1 Week Past", 100, "2025-01-20", "2025-02-03", null, Frequency.Weeks, 1);
            expense2.AccruedIsDirty = false;

            // Due today - should not advance
            var expense3 = EntityFactory.CreateExpense(_account, false, "Due Today", 100, "2025-01-27", "2025-02-10", null, Frequency.Weeks, 1);
            expense3.AccruedIsDirty = false;

            // Not yet due - should not advance
            var expense4 = EntityFactory.CreateExpense(_account, false, "Future Due", 100, "2025-02-03", "2025-02-17", null, Frequency.Weeks, 1);
            expense4.AccruedIsDirty = false;

            _calculator.Renew([expense1, expense2, expense3, expense4], advanceUntilDate);

            // expense1: 2025-01-20 -> 2025-01-27 -> 2025-02-03 -> 2025-02-10 -> 2025-02-17
            expense1.NextDue.Should().Be(new DateOnly(2025, 2, 17));
            expense1.AccrualStart.Should().Be(new DateOnly(2025, 2, 10));
            expense1.AccruedIsDirty.Should().BeTrue();

            // expense2: 2025-02-03 -> 2025-02-10 -> 2025-02-17
            expense2.NextDue.Should().Be(new DateOnly(2025, 2, 17));
            expense2.AccrualStart.Should().Be(new DateOnly(2025, 2, 10));
            expense2.AccruedIsDirty.Should().BeTrue();

            // expense3: 2025-02-10 -> 2025-02-17 (does renew once)
            expense3.NextDue.Should().Be(new DateOnly(2025, 2, 17));
            expense3.AccrualStart.Should().Be(new DateOnly(2025, 2, 10));
            expense3.AccruedIsDirty.Should().BeTrue();

            // expense4: No change
            expense4.NextDue.Should().Be(new DateOnly(2025, 2, 17));
            expense4.AccrualStart.Should().Be(new DateOnly(2025, 2, 3));
            expense4.AccruedIsDirty.Should().BeFalse();
        }

        [Fact]
        public void Should_Renew_Expenses_With_Monthly_Frequency()
        {
            // Includes multiple renewals where:
            // 1 expense is 3 months past due
            // 1 expense is 1 month past due
            // 1 expense is due today
            // 1 expense is not yet due

            var advanceUntilDate = new DateOnly(2025, 5, 15);

            // 3 months past due - should advance 3 times
            var expense1 = EntityFactory.CreateExpense(_account, false, "3 Months Past", 100, "2025-01-15", "2025-02-15", null, Frequency.Months, 1);
            expense1.AccruedIsDirty = false;

            // 1 month past due - should advance 1 time
            var expense2 = EntityFactory.CreateExpense(_account, false, "1 Month Past", 100, "2025-03-15", "2025-04-15", null, Frequency.Months, 1);
            expense2.AccruedIsDirty = false;

            // Due today - should not advance
            var expense3 = EntityFactory.CreateExpense(_account, false, "Due Today", 100, "2025-04-15", "2025-05-15", null, Frequency.Months, 1);
            expense3.AccruedIsDirty = false;

            // Not yet due - should not advance
            var expense4 = EntityFactory.CreateExpense(_account, false, "Future Due", 100, "2025-05-15", "2025-06-15", null, Frequency.Months, 1);
            expense4.AccruedIsDirty = false;

            _calculator.Renew([expense1, expense2, expense3, expense4], advanceUntilDate);

            // expense1: 2025-02-15 -> 2025-03-15 -> 2025-04-15 -> 2025-05-15 -> 2025-06-15
            expense1.NextDue.Should().Be(new DateOnly(2025, 6, 15));
            expense1.AccrualStart.Should().Be(new DateOnly(2025, 5, 15));
            expense1.AccruedIsDirty.Should().BeTrue();

            // expense2: 2025-04-15 -> 2025-05-15 -> 2025-06-15
            expense2.NextDue.Should().Be(new DateOnly(2025, 6, 15));
            expense2.AccrualStart.Should().Be(new DateOnly(2025, 5, 15));
            expense2.AccruedIsDirty.Should().BeTrue();

            // expense3: 2025-05-15 -> 2025-06-15 (does renew once)
            expense3.NextDue.Should().Be(new DateOnly(2025, 6, 15));
            expense3.AccrualStart.Should().Be(new DateOnly(2025, 5, 15));
            expense3.AccruedIsDirty.Should().BeTrue();

            // expense4: No change
            expense4.NextDue.Should().Be(new DateOnly(2025, 6, 15));
            expense4.AccrualStart.Should().Be(new DateOnly(2025, 5, 15));
            expense4.AccruedIsDirty.Should().BeFalse();
        }

        [Fact]
        public void Should_Renew_Expenses_With_Yearly_Frequency()
        {
            // Includes multiple renewals where:
            // 1 expense is 3 years past due
            // 1 expense is 1 year past due
            // 1 expense is due today
            // 1 expense is not yet due

            var advanceUntilDate = new DateOnly(2028, 3, 15);

            // 3 years past due - should advance 3 times
            var expense1 = EntityFactory.CreateExpense(_account, false, "3 Years Past", 100, "2024-03-15", "2025-03-15", null, Frequency.Years, 1);
            expense1.AccruedIsDirty = false;

            // 1 year past due - should advance 1 time
            var expense2 = EntityFactory.CreateExpense(_account, false, "1 Year Past", 100, "2026-03-15", "2027-03-15", null, Frequency.Years, 1);
            expense2.AccruedIsDirty = false;

            // Due today - should not advance
            var expense3 = EntityFactory.CreateExpense(_account, false, "Due Today", 100, "2027-03-15", "2028-03-15", null, Frequency.Years, 1);
            expense3.AccruedIsDirty = false;

            // Not yet due - should not advance
            var expense4 = EntityFactory.CreateExpense(_account, false, "Future Due", 100, "2028-03-15", "2029-03-15", null, Frequency.Years, 1);
            expense4.AccruedIsDirty = false;

            _calculator.Renew([expense1, expense2, expense3, expense4], advanceUntilDate);

            // expense1: 2025-03-15 -> 2026-03-15 -> 2027-03-15 -> 2028-03-15 -> 2029-03-15
            expense1.NextDue.Should().Be(new DateOnly(2029, 3, 15));
            expense1.AccrualStart.Should().Be(new DateOnly(2028, 3, 15));
            expense1.AccruedIsDirty.Should().BeTrue();

            // expense2: 2027-03-15 -> 2028-03-15 -> 2029-03-15
            expense2.NextDue.Should().Be(new DateOnly(2029, 3, 15));
            expense2.AccrualStart.Should().Be(new DateOnly(2028, 3, 15));
            expense2.AccruedIsDirty.Should().BeTrue();

            // expense3: 2028-03-15 -> 2029-03-15 (does renew once)
            expense3.NextDue.Should().Be(new DateOnly(2029, 3, 15));
            expense3.AccrualStart.Should().Be(new DateOnly(2028, 3, 15));
            expense3.AccruedIsDirty.Should().BeTrue();

            // expense4: No change
            expense4.NextDue.Should().Be(new DateOnly(2029, 3, 15));
            expense4.AccrualStart.Should().Be(new DateOnly(2028, 3, 15));
            expense4.AccruedIsDirty.Should().BeFalse();
        }

        [Fact]
        public void Should_Renew_Expense_With_Custom_FrequencyCount()
        {
            var advanceUntilDate = new DateOnly(2025, 2, 15);

            // Weekly with FrequencyCount = 2 (every 2 weeks = 14 days)
            var expense1 = EntityFactory.CreateExpense(_account, false, "BiWeekly", 100, "2025-01-01", "2025-01-01", null, Frequency.Weeks, 2);
            expense1.AccruedIsDirty = false;

            // Monthly with FrequencyCount = 3 (every 3 months)
            var expense2 = EntityFactory.CreateExpense(_account, false, "Quarterly", 200, "2024-11-15", "2025-02-15", null, Frequency.Months, 3);
            expense2.AccruedIsDirty = false;

            _calculator.Renew([expense1, expense2], advanceUntilDate);

            // expense1: 2025-01-01 -> 2025-01-15 -> 2025-01-29 -> 2025-02-12 -> 2025-02-26 (14 days later each time)
            expense1.NextDue.Should().Be(new DateOnly(2025, 2, 26));
            expense1.AccrualStart.Should().Be(new DateOnly(2025, 2, 12));
            expense1.AccruedIsDirty.Should().BeTrue();

            // expense2: 2025-02-15 -> 2025-05-15 (due on advanceUntilDate, renews once)
            expense2.NextDue.Should().Be(new DateOnly(2025, 5, 15));
            expense2.AccrualStart.Should().Be(new DateOnly(2025, 2, 15));
            expense2.AccruedIsDirty.Should().BeTrue();
        }

        [Fact]
        public void Should_Not_Renew_Expense_When_AdvanceUntilDate_Equals_EndDate()
        {

            var advanceUntilDate = new DateOnly(2025, 1, 20);
            var expense = EntityFactory.CreateExpense(_account, false, "Ending Expense", 100, "2025-01-01", "2025-01-10", "2025-01-20", Frequency.Months, 1);
            expense.AccruedIsDirty = false;

            var originalNextDue = expense.NextDue;

            _calculator.Renew([expense], advanceUntilDate);

            expense.NextDue.Should().Be(originalNextDue);
            expense.AccruedIsDirty.Should().BeFalse();
        }

        [Fact]
        public void Should_Renew_Multiple_Expenses_With_Different_Frequencies()
        {
            var advanceUntilDate = new DateOnly(2025, 2, 10);

            var dailyExpense = EntityFactory.CreateExpense(_account, false, "Daily", 10, "2025-01-01", "2025-02-05", null, Frequency.Days, 1);
            dailyExpense.AccruedIsDirty = false;

            var weeklyExpense = EntityFactory.CreateExpense(_account, false, "Weekly", 50, "2025-01-15", "2025-02-05", null, Frequency.Weeks, 1);
            weeklyExpense.AccruedIsDirty = false;

            var monthlyExpense = EntityFactory.CreateExpense(_account, false, "Monthly", 100, "2024-12-10", "2025-01-10", null, Frequency.Months, 1);
            monthlyExpense.AccruedIsDirty = false;

            _calculator.Renew([dailyExpense, weeklyExpense, monthlyExpense], advanceUntilDate);

            // Daily: 2025-02-05 -> 2025-02-06 -> ... -> 2025-02-10 -> 2025-02-11
            dailyExpense.NextDue.Should().Be(new DateOnly(2025, 2, 11));
            dailyExpense.AccrualStart.Should().Be(new DateOnly(2025, 2, 10));
            dailyExpense.AccruedIsDirty.Should().BeTrue();

            // Weekly: 2025-02-05 -> 2025-02-12 (7 days), renews past advanceUntilDate
            weeklyExpense.NextDue.Should().Be(new DateOnly(2025, 2, 12));
            weeklyExpense.AccrualStart.Should().Be(new DateOnly(2025, 2, 5));
            weeklyExpense.AccruedIsDirty.Should().BeTrue();

            // Monthly: 2025-01-10 -> 2025-02-10 -> 2025-03-10
            monthlyExpense.NextDue.Should().Be(new DateOnly(2025, 3, 10));
            monthlyExpense.AccrualStart.Should().Be(new DateOnly(2025, 2, 10));
            monthlyExpense.AccruedIsDirty.Should().BeTrue();
        }

        [Fact]
        public void Should_Stop_Renewing_When_Next_Due_Would_Exceed_EndDate()
        {
            var advanceUntilDate = new DateOnly(2025, 2, 15);

            // Expense should renew multiple times but stop before exceeding EndDate
            var expense = EntityFactory.CreateExpense(_account, false, "Limited Renewal", 100, "2025-01-01", "2025-01-01", "2025-02-20", Frequency.Weeks, 1);
            expense.AccruedIsDirty = false;

            _calculator.Renew([expense], advanceUntilDate);

            // 2025-01-01 -> ... -> 2025-02-05 -> 2025-02-12 (nextDue <= advanceUntilDate)
            // Then calculates 2025-02-19, which is <= endDate (2025-02-20), so updates to 2025-02-19
            // Loop exits because 2025-02-19 > 2025-02-15
            expense.NextDue.Should().Be(new DateOnly(2025, 2, 19));
            expense.AccrualStart.Should().Be(new DateOnly(2025, 2, 12));
            expense.AccruedIsDirty.Should().BeTrue();
        }

        [Fact]
        public void Should_Handle_Leap_Year_In_Yearly_Frequency()
        {
            var advanceUntilDate = new DateOnly(2025, 3, 1);

            // Expense starting on Feb 29 (leap year)
            var expense = EntityFactory.CreateExpense(_account, false, "Leap Year", 100, "2024-02-29", "2024-02-29", null, Frequency.Years, 1);
            expense.AccruedIsDirty = false;

            _calculator.Renew([expense], advanceUntilDate);

            // 2024-02-29 -> 2025-02-28 -> 2026-02-28 (365 days later in non-leap year, then 365 days again)
            expense.NextDue.Should().Be(new DateOnly(2026, 2, 28));
            expense.AccrualStart.Should().Be(new DateOnly(2025, 2, 28));
            expense.AccruedIsDirty.Should().BeTrue();
        }

        [Fact]
        public void Should_Not_Modify_Accrued_Amount_During_Renewal()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            expense.Accrued = 50.0;

            var advanceUntilDate = new DateOnly(2025, 1, 20);

            _calculator.Renew([expense], advanceUntilDate);

            // Accrued amount should not be modified by the renewal process
            expense.Accrued.Should().Be(50.0);
        }

        [Fact]
        public void Should_Renew_Expense_Due_On_AdvanceUntilDate_Minus_One()
        {
            var advanceUntilDate = new DateOnly(2025, 1, 20);
            var expense = EntityFactory.CreateExpense(_account, false, "Due Yesterday", 100, "2025-01-01", "2025-01-19", null, Frequency.Weeks, 1);

            _calculator.Renew([expense], advanceUntilDate);

            // Should renew to next week: 2025-01-19 -> 2025-01-26
            expense.NextDue.Should().Be(new DateOnly(2025, 1, 26));
            expense.AccrualStart.Should().Be(new DateOnly(2025, 1, 19));
            expense.AccruedIsDirty.Should().BeTrue();
        }

        [Fact]
        public void Should_Renew_When_NextDue_Equals_EndDate_Exactly()
        {
            var advanceUntilDate = new DateOnly(2025, 2, 15);

            // Expense where next renewal lands exactly on EndDate
            var expense = EntityFactory.CreateExpense(_account, false, "Exact EndDate", 100, "2025-01-01", "2025-01-08", "2025-02-12", Frequency.Weeks, 1);
            expense.AccruedIsDirty = false;

            _calculator.Renew([expense], advanceUntilDate);

            // Should renew to 2025-02-12 (which equals EndDate)
            expense.NextDue.Should().Be(new DateOnly(2025, 2, 12));
            expense.AccrualStart.Should().Be(new DateOnly(2025, 2, 5));
            expense.AccruedIsDirty.Should().BeTrue();
        }

        [Fact]
        public void Should_Not_Modify_LastAccruedUpdate_During_Renewal()
        {
            var lastAccruedUpdate = new DateOnly(2025, 1, 5);

            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            expense.LastAccruedUpdate = lastAccruedUpdate;

            var advanceUntilDate = new DateOnly(2025, 1, 20);

            _calculator.Renew([expense], advanceUntilDate);

            // LastAccruedUpdate should not be modified by the renewal process
            expense.LastAccruedUpdate.Should().Be(lastAccruedUpdate);
        }

        [Fact]
        public void Should_Not_Modify_Amount_And_Note_During_Renewal()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Test Expense", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            expense.Note = "Important note";

            var originalAmount = expense.Amount;
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            _calculator.Renew([expense], advanceUntilDate);

            // These fields should not be modified
            expense.Amount.Should().Be(originalAmount);
            expense.Note.Should().Be("Important note");
        }

        [Fact]
        public void Should_Handle_Month_End_Dates_Starting_Jan_31_With_Multiple_Renewals()
        {
            // Validates month-end date drift when an expense starting on the 31st renews through February
            var advanceUntilDate = new DateOnly(2025, 4, 15);

            var expense = EntityFactory.CreateExpense(_account, false, "Month End 31st", 100, "2025-01-31", "2025-01-31", null, Frequency.Months, 1);
            expense.AccruedIsDirty = false;

            _calculator.Renew([expense], advanceUntilDate);

            // Date drift due to .NET's AddMonths behavior:
            // Jan 31 -> Feb 28 (Feb has only 28 days in 2025)
            // Feb 28 -> Mar 28 (AddMonths from 28th stays on 28th)
            // Mar 28 -> Apr 28 (final renewal beyond advanceUntilDate)
            //
            // The expense permanently drifts from the 31st to the 28th
            
            expense.NextDue.Should().Be(new DateOnly(2025, 4, 28));
            expense.AccrualStart.Should().Be(new DateOnly(2025, 3, 28));
            expense.AccruedIsDirty.Should().BeTrue();
        }

        [Fact]
        public void Should_Handle_Month_End_31st_Renewing_Twice_Through_February()
        {
            // Validates that multiple renewals through February cause permanent date drift
            var advanceUntilDate = new DateOnly(2025, 3, 31);

            var expense = EntityFactory.CreateExpense(_account, false, "Jan 31 -> Feb -> Mar", 100, "2025-01-31", "2025-01-31", null, Frequency.Months, 1);
            
            _calculator.Renew([expense], advanceUntilDate);

            // Renewal sequence:
            // Jan 31 + 1 month = Feb 28 (Feb has only 28 days in 2025)
            // Feb 28 + 1 month = Mar 28 (subsequent renewals stay on 28th)
            // Mar 28 + 1 month = Apr 28 (final renewal beyond advanceUntilDate)
            
            // The expense permanently drifts from 31st to 28th
            expense.NextDue.Should().Be(new DateOnly(2025, 4, 28));
            expense.AccrualStart.Should().Be(new DateOnly(2025, 3, 28));
            expense.AccruedIsDirty.Should().BeTrue();
        }

        [Fact]
        public void Should_Handle_Large_Frequency_Counts()
        {
            var advanceUntilDate = new DateOnly(2027, 1, 15);

            // Semi-annual (every 6 months)
            var expense = EntityFactory.CreateExpense(_account, false, "Semi-Annual", 100, "2025-01-15", "2025-01-15", null, Frequency.Months, 6);
            expense.AccruedIsDirty = false;

            _calculator.Renew([expense], advanceUntilDate);

            // 2025-01-15 -> 2025-07-15 -> 2026-01-15 -> 2026-07-15 -> 2027-01-15 -> 2027-07-15
            expense.NextDue.Should().Be(new DateOnly(2027, 7, 15));
            expense.AccrualStart.Should().Be(new DateOnly(2027, 1, 15));
            expense.AccruedIsDirty.Should().BeTrue();
        }

        [Fact]
        public void Should_Handle_Collection_With_Mix_Of_Excluded_OneTime_And_Renewable()
        {
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            var excluded = EntityFactory.CreateExpense(_account, true, "Excluded", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            excluded.AccruedIsDirty = false;

            var oneTime = EntityFactory.CreateExpense(_account, false, "OneTime", 100, "2025-01-01", "2025-01-10", null, Frequency.OneTime, 1);
            oneTime.AccruedIsDirty = false;

            var renewable = EntityFactory.CreateExpense(_account, false, "Renewable", 100, "2025-01-01", "2025-01-10", null, Frequency.Weeks, 1);
            renewable.AccruedIsDirty = false;

            _calculator.Renew([excluded, oneTime, renewable], advanceUntilDate);

            // Only renewable should be renewed
            excluded.NextDue.Should().Be(new DateOnly(2025, 1, 10));
            excluded.AccruedIsDirty.Should().BeFalse();

            oneTime.NextDue.Should().Be(new DateOnly(2025, 1, 10));
            oneTime.AccruedIsDirty.Should().BeFalse();

            renewable.NextDue.Should().Be(new DateOnly(2025, 1, 24));
            renewable.AccruedIsDirty.Should().BeTrue();
        }

        [Fact]
        public void Should_Not_Renew_When_NextDue_Equals_EndDate_At_Start()
        {
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            // Expense where NextDue already equals EndDate (has reached its end)
            var expense = EntityFactory.CreateExpense(_account, false, "Already At EndDate", 100, "2025-01-01", "2025-01-15", "2025-01-15", Frequency.Weeks, 1);
            expense.AccruedIsDirty = false;

            var originalNextDue = expense.NextDue;
            var originalAccrualStart = expense.AccrualStart;

            _calculator.Renew([expense], advanceUntilDate);

            // Should not renew because NextDue == EndDate at the start
            expense.NextDue.Should().Be(originalNextDue);
            expense.AccrualStart.Should().Be(originalAccrualStart);
            expense.AccruedIsDirty.Should().BeFalse();
        }

        [Fact]
        public void Should_Not_Renew_When_NextDue_After_EndDate_At_Start()
        {
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            // Expense where NextDue is already past EndDate (shouldn't happen in normal flow, but validates the guard)
            var expense = EntityFactory.CreateExpense(_account, false, "Past EndDate", 100, "2025-01-01", "2025-01-18", "2025-01-15", Frequency.Weeks, 1);
            expense.AccruedIsDirty = false;

            var originalNextDue = expense.NextDue;
            var originalAccrualStart = expense.AccrualStart;

            _calculator.Renew([expense], advanceUntilDate);

            // Should not renew because NextDue > EndDate at the start
            expense.NextDue.Should().Be(originalNextDue);
            expense.AccrualStart.Should().Be(originalAccrualStart);
            expense.AccruedIsDirty.Should().BeFalse();
        }
    }
}
