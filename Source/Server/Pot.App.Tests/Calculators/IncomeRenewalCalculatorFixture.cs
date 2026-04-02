using Pot.App.Calculators;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Calculators;

public class IncomeRenewalCalculatorFixture : PotFixtureBase
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
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _calculator.Renew(null!, RenewalMode.Overdue, DateOnly.MinValue);
            });

            exception.ParamName.ShouldBe("incomes");
        }

        [Fact]
        public void Should_Handle_Empty_Income_List()
        {
            var asOfDate = new DateOnly(2025, 1, 20);

            Should.NotThrow(() =>
            {
                _calculator.Renew([], RenewalMode.Overdue, asOfDate);
            });
        }

        [Fact]
        public void Should_Not_Renew_Income_Excluded_From_Calcs()
        {
            var income = EntityFactory.CreateIncome(_account, true, "Excluded Income", 100, "2025-01-15", null, Frequency.Months, 1);
            var asOfDate = new DateOnly(2025, 1, 20);

            var originalNextDue = income.NextDue;

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            income.NextDue.ShouldBe(originalNextDue);
        }

        [Fact]
        public void Should_Not_Renew_One_Time_Income()
        {
            var income = EntityFactory.CreateIncome(_account, false, "OneTime Income", 100, "2025-01-15", null, Frequency.OneTime, 1);
            var asOfDate = new DateOnly(2025, 1, 20);

            var originalNextDue = income.NextDue;

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            income.NextDue.ShouldBe(originalNextDue);
        }

        [Fact]
        public void Should_Not_Renew_Income_With_EndDate_Before_AsOfDate()
        {
            var income = EntityFactory.CreateIncome(_account, false, "Ending Income", 100, "2025-01-10", "2025-01-15", Frequency.Months, 1);
            var asOfDate = new DateOnly(2025, 1, 20);

            var originalNextDue = income.NextDue;

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            income.NextDue.ShouldBe(originalNextDue);
        }

        [Fact]
        public void Should_Not_Renew_Income_With_NextDue_Date_After_EndDate()
        {
            var income = EntityFactory.CreateIncome(_account, false, "Limited Income", 100, "2025-01-10", "2025-01-20", Frequency.Months, 1);
            var asOfDate = new DateOnly(2025, 1, 15);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // Income should be renewed to 2025-02-10, but since that's after EndDate (2025-01-20), it should stop at 2025-01-10
            income.NextDue.ShouldBe(new DateOnly(2025, 1, 10));
        }

        [Fact]
        public void Should_Renew_Incomes_With_Daily_Frequency()
        {
            var asOfDate = new DateOnly(2025, 1, 20);

            // 3 days past due - should advance 3 times
            var income1 = EntityFactory.CreateIncome(_account, false, "3 Days Past", 100, "2025-01-17", null, Frequency.Days, 1);

            // 1 day past due - should advance 1 time
            var income2 = EntityFactory.CreateIncome(_account, false, "1 Day Past", 100, "2025-01-19", null, Frequency.Days, 1);

            // Due today - should renew once
            var income3 = EntityFactory.CreateIncome(_account, false, "Due Today", 100, "2025-01-20", null, Frequency.Days, 1);

            // Not yet due - should not advance
            var income4 = EntityFactory.CreateIncome(_account, false, "Future Due", 100, "2025-01-21", null, Frequency.Days, 1);

            _calculator.Renew([income1, income2, income3, income4], RenewalMode.Overdue, asOfDate);

            income1.NextDue.ShouldBe(new DateOnly(2025, 1, 21));
            income2.NextDue.ShouldBe(new DateOnly(2025, 1, 21));
            income3.NextDue.ShouldBe(new DateOnly(2025, 1, 21));
            income4.NextDue.ShouldBe(new DateOnly(2025, 1, 21));
        }

        [Fact]
        public void Should_Renew_Incomes_With_Weekly_Frequency()
        {
            var asOfDate = new DateOnly(2025, 2, 10);

            // 3 weeks past due - should advance 3 times
            var income1 = EntityFactory.CreateIncome(_account, false, "3 Weeks Past", 100, "2025-01-20", null, Frequency.Weeks, 1);

            // 1 week past due - should advance 1 time
            var income2 = EntityFactory.CreateIncome(_account, false, "1 Week Past", 100, "2025-02-03", null, Frequency.Weeks, 1);

            // Due today - should renew once
            var income3 = EntityFactory.CreateIncome(_account, false, "Due Today", 100, "2025-02-10", null, Frequency.Weeks, 1);

            // Not yet due - should not advance
            var income4 = EntityFactory.CreateIncome(_account, false, "Future Due", 100, "2025-02-17", null, Frequency.Weeks, 1);

            _calculator.Renew([income1, income2, income3, income4], RenewalMode.Overdue, asOfDate);

            income1.NextDue.ShouldBe(new DateOnly(2025, 2, 17));
            income2.NextDue.ShouldBe(new DateOnly(2025, 2, 17));
            income3.NextDue.ShouldBe(new DateOnly(2025, 2, 17));
            income4.NextDue.ShouldBe(new DateOnly(2025, 2, 17));
        }

        [Fact]
        public void Should_Renew_Incomes_With_Monthly_Frequency()
        {
            var asOfDate = new DateOnly(2025, 5, 15);

            // 3 months past due - should advance 3 times
            var income1 = EntityFactory.CreateIncome(_account, false, "3 Months Past", 100, "2025-02-15", null, Frequency.Months, 1);

            // 1 month past due - should advance 1 time
            var income2 = EntityFactory.CreateIncome(_account, false, "1 Month Past", 100, "2025-04-15", null, Frequency.Months, 1);

            // Due today - should renew once
            var income3 = EntityFactory.CreateIncome(_account, false, "Due Today", 100, "2025-05-15", null, Frequency.Months, 1);

            // Not yet due - should not advance
            var income4 = EntityFactory.CreateIncome(_account, false, "Future Due", 100, "2025-06-15", null, Frequency.Months, 1);

            _calculator.Renew([income1, income2, income3, income4], RenewalMode.Overdue, asOfDate);

            income1.NextDue.ShouldBe(new DateOnly(2025, 6, 15));
            income2.NextDue.ShouldBe(new DateOnly(2025, 6, 15));
            income3.NextDue.ShouldBe(new DateOnly(2025, 6, 15));
            income4.NextDue.ShouldBe(new DateOnly(2025, 6, 15));
        }

        [Fact]
        public void Should_Renew_Incomes_With_Yearly_Frequency()
        {
            var asOfDate = new DateOnly(2028, 3, 15);

            // 3 years past due - should advance 3 times
            var income1 = EntityFactory.CreateIncome(_account, false, "3 Years Past", 100, "2025-03-15", null, Frequency.Years, 1);

            // 1 year past due - should advance 1 time
            var income2 = EntityFactory.CreateIncome(_account, false, "1 Year Past", 100, "2027-03-15", null, Frequency.Years, 1);

            // Due today - should renew once
            var income3 = EntityFactory.CreateIncome(_account, false, "Due Today", 100, "2028-03-15", null, Frequency.Years, 1);

            // Not yet due - should not advance
            var income4 = EntityFactory.CreateIncome(_account, false, "Future Due", 100, "2029-03-15", null, Frequency.Years, 1);

            _calculator.Renew([income1, income2, income3, income4], RenewalMode.Overdue, asOfDate);

            income1.NextDue.ShouldBe(new DateOnly(2029, 3, 15));
            income2.NextDue.ShouldBe(new DateOnly(2029, 3, 15));
            income3.NextDue.ShouldBe(new DateOnly(2029, 3, 15));
            income4.NextDue.ShouldBe(new DateOnly(2029, 3, 15));
        }

        [Fact]
        public void Should_Renew_Income_With_Custom_FrequencyCount()
        {
            var asOfDate = new DateOnly(2025, 2, 15);

            // Weekly with FrequencyCount = 2 (every 2 weeks = 14 days)
            var income1 = EntityFactory.CreateIncome(_account, false, "BiWeekly", 100, "2025-01-01", null, Frequency.Weeks, 2);

            // Monthly with FrequencyCount = 3 (every 3 months)
            var income2 = EntityFactory.CreateIncome(_account, false, "Quarterly", 200, "2025-02-15", null, Frequency.Months, 3);

            _calculator.Renew([income1, income2], RenewalMode.Overdue, asOfDate);

            // income1: 2025-01-01 -> 2025-01-15 -> 2025-01-29 -> 2025-02-12 -> 2025-02-26
            income1.NextDue.ShouldBe(new DateOnly(2025, 2, 26));

            // income2: 2025-02-15 -> 2025-05-15 (due on asOfDate, renews once)
            income2.NextDue.ShouldBe(new DateOnly(2025, 5, 15));
        }

        [Fact]
        public void Should_Not_Renew_Income_When_AsOfDate_Equals_EndDate()
        {
            var asOfDate = new DateOnly(2025, 1, 20);

            var income = EntityFactory.CreateIncome(_account, false, "Ending Income", 100, "2025-01-10", "2025-01-20", Frequency.Months, 1);

            var originalNextDue = income.NextDue;

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            income.NextDue.ShouldBe(originalNextDue);
        }

        [Fact]
        public void Should_Renew_Multiple_Incomes_With_Different_Frequencies()
        {
            var asOfDate = new DateOnly(2025, 2, 10);

            var dailyIncome = EntityFactory.CreateIncome(_account, false, "Daily", 10, "2025-02-05", null, Frequency.Days, 1);
            var weeklyIncome = EntityFactory.CreateIncome(_account, false, "Weekly", 50, "2025-02-05", null, Frequency.Weeks, 1);
            var monthlyIncome = EntityFactory.CreateIncome(_account, false, "Monthly", 100, "2025-01-10", null, Frequency.Months, 1);

            _calculator.Renew([dailyIncome, weeklyIncome, monthlyIncome], RenewalMode.Overdue, asOfDate);

            // Daily: 2025-02-05 -> 2025-02-06 -> ... -> 2025-02-10 -> 2025-02-11
            dailyIncome.NextDue.ShouldBe(new DateOnly(2025, 2, 11));

            // Weekly: 2025-02-05 -> 2025-02-12
            weeklyIncome.NextDue.ShouldBe(new DateOnly(2025, 2, 12));

            // Monthly: 2025-01-10 -> 2025-02-10 -> 2025-03-10
            monthlyIncome.NextDue.ShouldBe(new DateOnly(2025, 3, 10));
        }

        [Fact]
        public void Should_Stop_Renewing_When_Next_Due_Would_Exceed_EndDate()
        {
            var asOfDate = new DateOnly(2025, 2, 15);

            // Income should renew multiple times but stop before exceeding EndDate
            var income = EntityFactory.CreateIncome(_account, false, "Limited Renewal", 100, "2025-01-01", "2025-02-20", Frequency.Weeks, 1);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // 2025-01-01 -> ... -> 2025-02-05 -> 2025-02-12 -> 2025-02-19 (should update to 2025-02-19)
            income.NextDue.ShouldBe(new DateOnly(2025, 2, 19));
        }

        [Fact]
        public void Should_Handle_Leap_Year_In_Yearly_Frequency()
        {
            var asOfDate = new DateOnly(2025, 3, 1);

            // Income starting on Feb 29 (leap year)
            var income = EntityFactory.CreateIncome(_account, false, "Leap Year", 100, "2024-02-29", null, Frequency.Years, 1);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // 2024-02-29 -> 2025-02-28 -> 2026-02-28
            income.NextDue.ShouldBe(new DateOnly(2026, 2, 28));
        }

        [Fact]
        public void Should_Not_Modify_Amount_And_Note_During_Renewal()
        {
            var income = EntityFactory.CreateIncome(_account, false, "Test Income", 100, "2025-01-10", null, Frequency.Weeks, 1);
            income.Note = "Important note";

            var originalAmount = income.Amount;
            var asOfDate = new DateOnly(2025, 1, 20);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // These fields should not be modified
            income.Amount.ShouldBe(originalAmount);
            income.Note.ShouldBe("Important note");
        }

        [Fact]
        public void Should_Handle_Month_End_Dates_Starting_Jan_31_With_Multiple_Renewals()
        {
            // Validates month-end date drift when income starting on the 31st renews through February
            var asOfDate = new DateOnly(2025, 4, 15);

            var income = EntityFactory.CreateIncome(_account, false, "Month End 31st", 100, "2025-01-31", null, Frequency.Months, 1);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // Date drift due to .NET's AddMonths behavior:
            // Jan 31 -> Feb 28 (Feb has only 28 days in 2025)
            // Feb 28 -> Mar 28 (AddMonths from 28th stays on 28th)
            // Mar 28 -> Apr 28 (final renewal beyond asOfDate)
            //
            // The income permanently drifts from the 31st to the 28th

            income.NextDue.ShouldBe(new DateOnly(2025, 4, 28));
        }

        [Fact]
        public void Should_Handle_Month_End_31st_Renewing_Twice_Through_February()
        {
            // Validates that multiple renewals through February cause permanent date drift
            var asOfDate = new DateOnly(2025, 3, 31);

            var income = EntityFactory.CreateIncome(_account, false, "Jan 31 -> Feb -> Mar", 100, "2025-01-31", null, Frequency.Months, 1);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // Renewal sequence:
            // Jan 31 + 1 month = Feb 28 (Feb has only 28 days in 2025)
            // Feb 28 + 1 month = Mar 28 (subsequent renewals stay on 28th)
            // Mar 28 + 1 month = Apr 28 (final renewal beyond asOfDate)

            // The income permanently drifts from 31st to 28th
            income.NextDue.ShouldBe(new DateOnly(2025, 4, 28));
        }

        [Fact]
        public void Should_Keep_End_Of_Month_Cadence_Through_February()
        {
            var asOfDate = new DateOnly(2025, 3, 31);

            var income = EntityFactory.CreateIncome(_account, false, "End Of Month", 100, "2025-01-31", null, Frequency.EndOfMonth, 1);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // Jan 31 -> Feb 28 -> Mar 31 -> Apr 30 (final renewal beyond asOfDate)
            income.NextDue.ShouldBe(new DateOnly(2025, 4, 30));
        }

        [Fact]
        public void Should_Handle_Large_Frequency_Counts()
        {
            var asOfDate = new DateOnly(2027, 1, 15);

            // Semi-annual (every 6 months)
            var income = EntityFactory.CreateIncome(_account, false, "Semi-Annual", 100, "2025-01-15", null, Frequency.Months, 6);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // 2025-01-15 -> 2025-07-15 -> 2026-01-15 -> 2026-07-15 -> 2027-01-15 -> 2027-07-15
            income.NextDue.ShouldBe(new DateOnly(2027, 7, 15));
        }

        [Fact]
        public void Should_Handle_Collection_With_Mix_Of_Excluded_OneTime_And_Renewable()
        {
            var asOfDate = new DateOnly(2025, 1, 20);

            var excluded = EntityFactory.CreateIncome(_account, true, "Excluded", 100, "2025-01-10", null, Frequency.Weeks, 1);
            var oneTime = EntityFactory.CreateIncome(_account, false, "OneTime", 100, "2025-01-10", null, Frequency.OneTime, 1);
            var renewable = EntityFactory.CreateIncome(_account, false, "Renewable", 100, "2025-01-10", null, Frequency.Weeks, 1);

            _calculator.Renew([excluded, oneTime, renewable], RenewalMode.Overdue, asOfDate);

            // Only renewable should be renewed
            excluded.NextDue.ShouldBe(new DateOnly(2025, 1, 10));
            oneTime.NextDue.ShouldBe(new DateOnly(2025, 1, 10));
            renewable.NextDue.ShouldBe(new DateOnly(2025, 1, 24));
        }

        [Fact]
        public void Should_Renew_Income_Due_On_AsOfDate_Minus_One()
        {
            var asOfDate = new DateOnly(2025, 1, 20);

            var income = EntityFactory.CreateIncome(_account, false, "Due Yesterday", 100, "2025-01-19", null, Frequency.Weeks, 1);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // Should renew to next week: 2025-01-19 -> 2025-01-26
            income.NextDue.ShouldBe(new DateOnly(2025, 1, 26));
        }

        [Fact]
        public void Should_Renew_When_NextDue_Equals_EndDate_Exactly()
        {
            var asOfDate = new DateOnly(2025, 2, 15);
            // Income where next renewal lands exactly on EndDate
            var income = EntityFactory.CreateIncome(_account, false, "Exact EndDate", 100, "2025-01-08", "2025-02-12", Frequency.Weeks, 1);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // Should renew to 2025-02-12 (which equals EndDate)
            income.NextDue.ShouldBe(new DateOnly(2025, 2, 12));
        }

        [Fact]
        public void Should_Not_Renew_When_NextDue_Equals_EndDate_At_Start()
        {
            var asOfDate = new DateOnly(2025, 1, 20);

            // Income where NextDue already equals EndDate (has reached its end)
            var income = EntityFactory.CreateIncome(_account, false, "Already At EndDate", 100, "2025-01-15", "2025-01-15", Frequency.Weeks, 1);

            var originalNextDue = income.NextDue;

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // Should not renew because NextDue == EndDate at the start
            income.NextDue.ShouldBe(originalNextDue);
        }

        [Fact]
        public void Should_Not_Renew_When_NextDue_After_EndDate_At_Start()
        {
            var asOfDate = new DateOnly(2025, 1, 20);

            // Income where NextDue is already past EndDate (shouldn't happen in normal flow, but validates the guard)
            var income = EntityFactory.CreateIncome(_account, false, "Past EndDate", 100, "2025-01-18", "2025-01-15", Frequency.Weeks, 1);

            var originalNextDue = income.NextDue;

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // Should not renew because NextDue > EndDate at the start
            income.NextDue.ShouldBe(originalNextDue);
        }
    }

    public class Renew_Future : IncomeRenewalCalculatorFixture
    {
        private readonly AccountEntity _account;
        private readonly IncomeRenewalCalculator _calculator;

        public Renew_Future()
        {
            _account = Create<AccountEntity>();
            _calculator = new IncomeRenewalCalculator();
        }

        [Fact]
        public void Should_Advance_Weekly_Income_Exactly_Once()
        {
            // Income due in the future (Feb 20)
            var income = EntityFactory.CreateIncome(_account, false, "Weekly Income", 500, "2025-02-20", null, Frequency.Weeks, 1);

            var asOfDate = new DateOnly(2025, 2, 10); // Today is Feb 10

            _calculator.Renew([income], RenewalMode.Future, asOfDate);

            // Should advance exactly once: Feb 20 + 7 days = Feb 27
            income.NextDue.ShouldBe(new DateOnly(2025, 2, 27));
        }

        [Fact]
        public void Should_Advance_Monthly_Income_Exactly_Once()
        {
            // Income due in the future (Mar 15)
            var income = EntityFactory.CreateIncome(_account, false, "Monthly Income", 3000, "2025-03-15", null, Frequency.Months, 1);

            var asOfDate = new DateOnly(2025, 2, 10); // Today is Feb 10

            _calculator.Renew([income], RenewalMode.Future, asOfDate);

            // Should advance exactly once: Mar 15 + 1 month = Apr 15
            income.NextDue.ShouldBe(new DateOnly(2025, 4, 15));
        }

        [Fact]
        public void Should_Advance_Yearly_Income_Exactly_Once()
        {
            // Income due in the future (Dec 25, 2025)
            var income = EntityFactory.CreateIncome(_account, false, "Yearly Bonus", 10000, "2025-12-25", null, Frequency.Years, 1);

            var asOfDate = new DateOnly(2025, 2, 10); // Today is Feb 10

            _calculator.Renew([income], RenewalMode.Future, asOfDate);

            // Should advance exactly once: Dec 25, 2025 + 1 year = Dec 25, 2026
            income.NextDue.ShouldBe(new DateOnly(2026, 12, 25));
        }

        [Fact]
        public void Should_Not_Advance_Beyond_EndDate()
        {
            // Income due in the future with EndDate that would be exceeded
            var income = EntityFactory.CreateIncome(_account, false, "Ending Income", 500, "2025-02-20", "2025-02-25", Frequency.Weeks, 1);

            var originalNextDue = income.NextDue;

            var asOfDate = new DateOnly(2025, 2, 10);

            _calculator.Renew([income], RenewalMode.Future, asOfDate);

            // Next due would be Feb 27, but that exceeds EndDate of Feb 25
            // So income should NOT be renewed
            income.NextDue.ShouldBe(originalNextDue);
        }

        [Fact]
        public void Should_Not_Renew_OneTime_Income_In_Future_Mode()
        {
            var income = EntityFactory.CreateIncome(_account, false, "OneTime Income", 1000, "2025-02-20", null, Frequency.OneTime, 1);

            var originalNextDue = income.NextDue;

            var asOfDate = new DateOnly(2025, 2, 10);

            _calculator.Renew([income], RenewalMode.Future, asOfDate);

            // OneTime incomes should never renew
            income.NextDue.ShouldBe(originalNextDue);
        }

        [Fact]
        public void Should_Not_Renew_Excluded_Income_In_Future_Mode()
        {
            var income = EntityFactory.CreateIncome(_account, true, "Excluded Income", 500, "2025-02-20", null, Frequency.Months, 1);

            var originalNextDue = income.NextDue;

            var asOfDate = new DateOnly(2025, 2, 10);

            _calculator.Renew([income], RenewalMode.Future, asOfDate);

            // Excluded incomes should not renew
            income.NextDue.ShouldBe(originalNextDue);
        }

        [Fact]
        public void Should_Advance_Multiple_Future_Incomes_With_Different_Frequencies()
        {
            var weeklyIncome = EntityFactory.CreateIncome(_account, false, "Weekly", 500, "2025-02-15", null, Frequency.Weeks, 1);
            var monthlyIncome = EntityFactory.CreateIncome(_account, false, "Monthly", 3000, "2025-03-01", null, Frequency.Months, 1);
            var yearlyIncome = EntityFactory.CreateIncome(_account, false, "Yearly", 10000, "2025-12-25", null, Frequency.Years, 1);

            var asOfDate = new DateOnly(2025, 2, 10);

            _calculator.Renew([weeklyIncome, monthlyIncome, yearlyIncome], RenewalMode.Future, asOfDate);

            // Weekly: Feb 15 + 7 days = Feb 22
            weeklyIncome.NextDue.ShouldBe(new DateOnly(2025, 2, 22));

            // Monthly: Mar 1 + 1 month = Apr 1
            monthlyIncome.NextDue.ShouldBe(new DateOnly(2025, 4, 1));

            // Yearly: Dec 25, 2025 + 1 year = Dec 25, 2026
            yearlyIncome.NextDue.ShouldBe(new DateOnly(2026, 12, 25));
        }
    }

    public class Renew_Overdue : IncomeRenewalCalculatorFixture
    {
        private readonly AccountEntity _account;
        private readonly IncomeRenewalCalculator _calculator;

        public Renew_Overdue()
        {
            _account = Create<AccountEntity>();
            _calculator = new IncomeRenewalCalculator();
        }

        [Fact]
        public void Should_Advance_Single_Period_Overdue_Income()
        {
            // Income overdue by one period (due Feb 3, today is Feb 10)
            var income = EntityFactory.CreateIncome(_account, false, "Overdue Income", 500, "2025-02-03", null, Frequency.Weeks, 1);

            var asOfDate = new DateOnly(2025, 2, 10);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // Should advance to first date after Feb 10: Feb 3 -> Feb 10 -> Feb 17
            income.NextDue.ShouldBe(new DateOnly(2025, 2, 17));
        }

        [Fact]
        public void Should_Advance_Multiple_Periods_Overdue_Income()
        {
            // Income overdue by multiple periods (due Jan 1, today is Feb 10)
            var income = EntityFactory.CreateIncome(_account, false, "Very Overdue", 500, "2025-01-01", null, Frequency.Weeks, 1);

            var asOfDate = new DateOnly(2025, 2, 10);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // Should catch up through multiple iterations: Jan 1 -> Jan 8 -> Jan 15 -> ... -> Feb 12
            income.NextDue.ShouldBe(new DateOnly(2025, 2, 12));
        }

        [Fact]
        public void Should_Advance_Income_Due_Today()
        {
            // Income due today
            var asOfDate = new DateOnly(2025, 2, 10);
            var income = EntityFactory.CreateIncome(_account, false, "Due Today", 500, "2025-02-10", null, Frequency.Weeks, 1);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // Due today should be treated as overdue and advance
            income.NextDue.ShouldBe(new DateOnly(2025, 2, 17));
        }

        [Fact]
        public void Should_Not_Renew_When_Overdue_Income_Next_Period_Exceeds_EndDate()
        {
            // Income is overdue but the next period would exceed its end date
            var asOfDate = new DateOnly(2025, 2, 15);
            var income = EntityFactory.CreateIncome(_account, false, "Overdue with near EndDate", 500, "2025-01-01", "2025-01-25", Frequency.Months, 1);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // Should remain unchanged since next period (2025-02-01) exceeds EndDate (2025-01-25)
            income.NextDue.ShouldBe(new DateOnly(2025, 1, 1), "income should not advance beyond its end date");
        }

        [Fact]
        public void Should_Stop_At_Intermediate_Renewal_When_Next_Period_Would_Exceed_EndDate_With_Weeks()
        {
            // Income is overdue by multiple weeks, but would exceed EndDate on an intermediate iteration
            // This tests the scenario where the first iteration succeeds, but a later one would exceed EndDate
            var asOfDate = new DateOnly(2025, 2, 15);
            var income = EntityFactory.CreateIncome(_account, false, "Multi-Week Overdue Near End", 500, "2025-01-15", "2025-02-20", Frequency.Weeks, 1);

            _calculator.Renew([income], RenewalMode.Overdue, asOfDate);

            // Sequence: Jan 15 -> Jan 22 -> Jan 29 -> Feb 5 -> Feb 12 -> Feb 19 (would exceed Feb 20)
            // Should stop at Feb 19
            income.NextDue.ShouldBe(new DateOnly(2025, 2, 19), "should advance through multiple periods but stop before exceeding end date");
        }
    }
}
