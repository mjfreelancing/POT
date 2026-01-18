using AllOverIt.Fixture.Extensions;
using FluentAssertions;
using Pot.App.Calculators;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;

namespace Pot.App.Tests.Calculators;

public class IncomeRenewalCalculatorFixture : CalculatorFixtureBase
{
    public IncomeRenewalCalculatorFixture()
    {
        CustomizeEnumerations();
        OmitRecursionBehavior();
    }

    public class Renew : IncomeRenewalCalculatorFixture
    {
        private readonly AccountEntity _account;
        private readonly IncomeRenewalCalculator _calculator;

        public Renew()
        {
            _account = Create<AccountEntity>();
            _calculator = new IncomeRenewalCalculator();
        }

        [Fact]
        public void Should_Throw_When_Incomess_Null()
        {
            Invoking(() =>
            {
                _calculator.Renew(null!, DateOnly.MinValue);
            })
            .Should()
            .Throw<ArgumentNullException>()
            .WithNamedMessageWhenNull("incomes");
        }

        [Fact]
        public void Should_Handle_Empty_Income_List()
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
        public void Should_Not_Renew_Income_Excluded_From_Calcs()
        {
            var income = CreateIncome(_account, true, "Excluded Income", 100, "2025-01-15", null, Frequency.Months, 1);
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            var originalNextDue = income.NextDue;

            _calculator.Renew([income], advanceUntilDate);

            income.NextDue.Should().Be(originalNextDue);
        }

        [Fact]
        public void Should_Not_Renew_One_Time_Income()
        {
            var income = CreateIncome(_account, false, "OneTime Income", 100, "2025-01-15", null, Frequency.OneTime, 1);
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            var originalNextDue = income.NextDue;

            _calculator.Renew([income], advanceUntilDate);

            income.NextDue.Should().Be(originalNextDue);
        }

        [Fact]
        public void Should_Not_Renew_Income_With_EndDate_Before_AdvanceUntilDate()
        {
            var income = CreateIncome(_account, false, "Ending Income", 100, "2025-01-10", "2025-01-15", Frequency.Months, 1);
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            var originalNextDue = income.NextDue;

            _calculator.Renew([income], advanceUntilDate);

            income.NextDue.Should().Be(originalNextDue);
        }

        [Fact]
        public void Should_Not_Renew_Income_With_NextDue_Date_After_EndDate()
        {
            var income = CreateIncome(_account, false, "Limited Income", 100, "2025-01-10", "2025-01-20", Frequency.Months, 1);
            var advanceUntilDate = new DateOnly(2025, 1, 15);

            _calculator.Renew([income], advanceUntilDate);

            // Income should be renewed to 2025-02-10, but since that's after EndDate (2025-01-20), it should stop at 2025-01-10
            income.NextDue.Should().Be(new DateOnly(2025, 1, 10));
        }

        [Fact]
        public void Should_Renew_Incomes_With_Daily_Frequency()
        {
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            // 3 days past due - should advance 3 times
            var income1 = CreateIncome(_account, false, "3 Days Past", 100, "2025-01-17", null, Frequency.Days, 1);

            // 1 day past due - should advance 1 time
            var income2 = CreateIncome(_account, false, "1 Day Past", 100, "2025-01-19", null, Frequency.Days, 1);

            // Due today - should renew once
            var income3 = CreateIncome(_account, false, "Due Today", 100, "2025-01-20", null, Frequency.Days, 1);

            // Not yet due - should not advance
            var income4 = CreateIncome(_account, false, "Future Due", 100, "2025-01-21", null, Frequency.Days, 1);

            _calculator.Renew([income1, income2, income3, income4], advanceUntilDate);

            income1.NextDue.Should().Be(new DateOnly(2025, 1, 21));
            income2.NextDue.Should().Be(new DateOnly(2025, 1, 21));
            income3.NextDue.Should().Be(new DateOnly(2025, 1, 21));
            income4.NextDue.Should().Be(new DateOnly(2025, 1, 21));
        }

        [Fact]
        public void Should_Renew_Incomes_With_Weekly_Frequency()
        {
            var advanceUntilDate = new DateOnly(2025, 2, 10);

            // 3 weeks past due - should advance 3 times
            var income1 = CreateIncome(_account, false, "3 Weeks Past", 100, "2025-01-20", null, Frequency.Weeks, 1);

            // 1 week past due - should advance 1 time
            var income2 = CreateIncome(_account, false, "1 Week Past", 100, "2025-02-03", null, Frequency.Weeks, 1);

            // Due today - should renew once
            var income3 = CreateIncome(_account, false, "Due Today", 100, "2025-02-10", null, Frequency.Weeks, 1);

            // Not yet due - should not advance
            var income4 = CreateIncome(_account, false, "Future Due", 100, "2025-02-17", null, Frequency.Weeks, 1);

            _calculator.Renew([income1, income2, income3, income4], advanceUntilDate);

            income1.NextDue.Should().Be(new DateOnly(2025, 2, 17));
            income2.NextDue.Should().Be(new DateOnly(2025, 2, 17));
            income3.NextDue.Should().Be(new DateOnly(2025, 2, 17));
            income4.NextDue.Should().Be(new DateOnly(2025, 2, 17));
        }

        [Fact]
        public void Should_Renew_Incomes_With_Monthly_Frequency()
        {
            var advanceUntilDate = new DateOnly(2025, 5, 15);

            // 3 months past due - should advance 3 times
            var income1 = CreateIncome(_account, false, "3 Months Past", 100, "2025-02-15", null, Frequency.Months, 1);

            // 1 month past due - should advance 1 time
            var income2 = CreateIncome(_account, false, "1 Month Past", 100, "2025-04-15", null, Frequency.Months, 1);

            // Due today - should renew once
            var income3 = CreateIncome(_account, false, "Due Today", 100, "2025-05-15", null, Frequency.Months, 1);

            // Not yet due - should not advance
            var income4 = CreateIncome(_account, false, "Future Due", 100, "2025-06-15", null, Frequency.Months, 1);

            _calculator.Renew([income1, income2, income3, income4], advanceUntilDate);

            income1.NextDue.Should().Be(new DateOnly(2025, 6, 15));
            income2.NextDue.Should().Be(new DateOnly(2025, 6, 15));
            income3.NextDue.Should().Be(new DateOnly(2025, 6, 15));
            income4.NextDue.Should().Be(new DateOnly(2025, 6, 15));
        }

        [Fact]
        public void Should_Renew_Incomes_With_Yearly_Frequency()
        {
            var advanceUntilDate = new DateOnly(2028, 3, 15);

            // 3 years past due - should advance 3 times
            var income1 = CreateIncome(_account, false, "3 Years Past", 100, "2025-03-15", null, Frequency.Years, 1);

            // 1 year past due - should advance 1 time
            var income2 = CreateIncome(_account, false, "1 Year Past", 100, "2027-03-15", null, Frequency.Years, 1);

            // Due today - should renew once
            var income3 = CreateIncome(_account, false, "Due Today", 100, "2028-03-15", null, Frequency.Years, 1);

            // Not yet due - should not advance
            var income4 = CreateIncome(_account, false, "Future Due", 100, "2029-03-15", null, Frequency.Years, 1);

            _calculator.Renew([income1, income2, income3, income4], advanceUntilDate);

            income1.NextDue.Should().Be(new DateOnly(2029, 3, 15));
            income2.NextDue.Should().Be(new DateOnly(2029, 3, 15));
            income3.NextDue.Should().Be(new DateOnly(2029, 3, 15));
            income4.NextDue.Should().Be(new DateOnly(2029, 3, 15));
        }

        [Fact]
        public void Should_Renew_Income_With_Custom_FrequencyCount()
        {
            var advanceUntilDate = new DateOnly(2025, 2, 15);

            // Weekly with FrequencyCount = 2 (every 2 weeks = 14 days)
            var income1 = CreateIncome(_account, false, "BiWeekly", 100, "2025-01-01", null, Frequency.Weeks, 2);

            // Monthly with FrequencyCount = 3 (every 3 months)
            var income2 = CreateIncome(_account, false, "Quarterly", 200, "2025-02-15", null, Frequency.Months, 3);

            _calculator.Renew([income1, income2], advanceUntilDate);

            // income1: 2025-01-01 -> 2025-01-15 -> 2025-01-29 -> 2025-02-12 -> 2025-02-26
            income1.NextDue.Should().Be(new DateOnly(2025, 2, 26));

            // income2: 2025-02-15 -> 2025-05-15 (due on advanceUntilDate, renews once)
            income2.NextDue.Should().Be(new DateOnly(2025, 5, 15));
        }

        [Fact]
        public void Should_Not_Renew_Income_When_AdvanceUntilDate_Equals_EndDate()
        {
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            var income = CreateIncome(_account, false, "Ending Income", 100, "2025-01-10", "2025-01-20", Frequency.Months, 1);

            var originalNextDue = income.NextDue;

            _calculator.Renew([income], advanceUntilDate);

            income.NextDue.Should().Be(originalNextDue);
        }

        [Fact]
        public void Should_Renew_Multiple_Incomes_With_Different_Frequencies()
        {
            var advanceUntilDate = new DateOnly(2025, 2, 10);

            var dailyIncome = CreateIncome(_account, false, "Daily", 10, "2025-02-05", null, Frequency.Days, 1);
            var weeklyIncome = CreateIncome(_account, false, "Weekly", 50, "2025-02-05", null, Frequency.Weeks, 1);
            var monthlyIncome = CreateIncome(_account, false, "Monthly", 100, "2025-01-10", null, Frequency.Months, 1);

            _calculator.Renew([dailyIncome, weeklyIncome, monthlyIncome], advanceUntilDate);

            // Daily: 2025-02-05 -> 2025-02-06 -> ... -> 2025-02-10 -> 2025-02-11
            dailyIncome.NextDue.Should().Be(new DateOnly(2025, 2, 11));

            // Weekly: 2025-02-05 -> 2025-02-12
            weeklyIncome.NextDue.Should().Be(new DateOnly(2025, 2, 12));

            // Monthly: 2025-01-10 -> 2025-02-10 -> 2025-03-10
            monthlyIncome.NextDue.Should().Be(new DateOnly(2025, 3, 10));
        }

        [Fact]
        public void Should_Stop_Renewing_When_Next_Due_Would_Exceed_EndDate()
        {
            var advanceUntilDate = new DateOnly(2025, 2, 15);

            // Income should renew multiple times but stop before exceeding EndDate
            var income = CreateIncome(_account, false, "Limited Renewal", 100, "2025-01-01", "2025-02-20", Frequency.Weeks, 1);

            _calculator.Renew([income], advanceUntilDate);

            // 2025-01-01 -> ... -> 2025-02-05 -> 2025-02-12 -> 2025-02-19 (should update to 2025-02-19)
            income.NextDue.Should().Be(new DateOnly(2025, 2, 19));
        }

        [Fact]
        public void Should_Handle_Leap_Year_In_Yearly_Frequency()
        {
            var advanceUntilDate = new DateOnly(2025, 3, 1);

            // Income starting on Feb 29 (leap year)
            var income = CreateIncome(_account, false, "Leap Year", 100, "2024-02-29", null, Frequency.Years, 1);

            _calculator.Renew([income], advanceUntilDate);

            // 2024-02-29 -> 2025-02-28 -> 2026-02-28
            income.NextDue.Should().Be(new DateOnly(2026, 2, 28));
        }

        [Fact]
        public void Should_Not_Modify_Amount_And_Note_During_Renewal()
        {
            var income = CreateIncome(_account, false, "Test Income", 100, "2025-01-10", null, Frequency.Weeks, 1);
            income.Note = "Important note";

            var originalAmount = income.Amount;
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            _calculator.Renew([income], advanceUntilDate);

            // These fields should not be modified
            income.Amount.Should().Be(originalAmount);
            income.Note.Should().Be("Important note");
        }

        [Fact]
        public void Should_Handle_Month_End_Dates_Correctly()
        {
            var advanceUntilDate = new DateOnly(2025, 4, 15);

            // Income starting on Jan 31 - see how it handles Feb
            var income = CreateIncome(_account, false, "Month End", 100, "2025-01-31", null, Frequency.Months, 1);

            _calculator.Renew([income], advanceUntilDate);

            // Jan 31 -> Feb 28 -> Mar 31 -> Apr 30 (next would be beyond advanceDate)
            income.NextDue.Should().BeAfter(advanceUntilDate);
        }

        [Fact]
        public void Should_Handle_Large_Frequency_Counts()
        {
            var advanceUntilDate = new DateOnly(2027, 1, 15);

            // Semi-annual (every 6 months)
            var income = CreateIncome(_account, false, "Semi-Annual", 100, "2025-01-15", null, Frequency.Months, 6);

            _calculator.Renew([income], advanceUntilDate);

            // 2025-01-15 -> 2025-07-15 -> 2026-01-15 -> 2026-07-15 -> 2027-01-15 -> 2027-07-15
            income.NextDue.Should().Be(new DateOnly(2027, 7, 15));
        }

        [Fact]
        public void Should_Handle_Collection_With_Mix_Of_Excluded_OneTime_And_Renewable()
        {
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            var excluded = CreateIncome(_account, true, "Excluded", 100, "2025-01-10", null, Frequency.Weeks, 1);
            var oneTime = CreateIncome(_account, false, "OneTime", 100, "2025-01-10", null, Frequency.OneTime, 1);
            var renewable = CreateIncome(_account, false, "Renewable", 100, "2025-01-10", null, Frequency.Weeks, 1);

            _calculator.Renew([excluded, oneTime, renewable], advanceUntilDate);

            // Only renewable should be renewed
            excluded.NextDue.Should().Be(new DateOnly(2025, 1, 10));
            oneTime.NextDue.Should().Be(new DateOnly(2025, 1, 10));
            renewable.NextDue.Should().Be(new DateOnly(2025, 1, 24));
        }

        [Fact]
        public void Should_Renew_Income_Due_On_AdvanceUntilDate_Minus_One()
        {
            var advanceUntilDate = new DateOnly(2025, 1, 20);

            var income = CreateIncome(_account, false, "Due Yesterday", 100, "2025-01-19", null, Frequency.Weeks, 1);

            _calculator.Renew([income], advanceUntilDate);

            // Should renew to next week: 2025-01-19 -> 2025-01-26
            income.NextDue.Should().Be(new DateOnly(2025, 1, 26));
        }

        [Fact]
        public void Should_Renew_When_NextDue_Equals_EndDate_Exactly()
        {
            var advanceUntilDate = new DateOnly(2025, 2, 15);

            // Income where next renewal lands exactly on EndDate
            var income = CreateIncome(_account, false, "Exact EndDate", 100, "2025-01-08", "2025-02-12", Frequency.Weeks, 1);

            _calculator.Renew([income], advanceUntilDate);

            // Should renew to 2025-02-12 (which equals EndDate)
            income.NextDue.Should().Be(new DateOnly(2025, 2, 12));
        }
    }
}
