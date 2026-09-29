using Pot.App.Calculators;
using Pot.Shared.Enumerations;
using Pot.Shared.Models;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Calculators;

public class IncomeRenewalFoldFixture : PotFixtureBase
{
    private static readonly DateOnly AsOfDate = new(2025, 1, 29);

    private readonly IncomeRenewalFold _fold = new();

    public class Renew : IncomeRenewalFoldFixture
    {
        [Fact]
        public void Should_Not_Advance_An_Excluded_Income()
        {
            var schedule = CreateSchedule(nextDue: "2025-01-01", excludeFromCalcs: true);

            var renewed = _fold.Renew(schedule, RenewalMode.Overdue, AsOfDate);

            renewed.ShouldBe(schedule);
        }

        [Fact]
        public void Should_Not_Advance_A_One_Time_Income()
        {
            var schedule = CreateSchedule(nextDue: "2025-01-01", frequency: Frequency.OneTime, frequencyCount: 0);

            var renewed = _fold.Renew(schedule, RenewalMode.Overdue, AsOfDate);

            renewed.ShouldBe(schedule);
        }

        [Fact]
        public void Should_Not_Advance_An_Income_That_Has_Reached_Its_End_Date()
        {
            var schedule = CreateSchedule(nextDue: "2025-01-15", endDate: "2025-01-15");

            var renewed = _fold.Renew(schedule, RenewalMode.Overdue, AsOfDate);

            renewed.ShouldBe(schedule);
        }

        [Fact]
        public void Should_Advance_Exactly_One_Period_In_Future_Mode()
        {
            var schedule = CreateSchedule(nextDue: "2025-02-20", frequency: Frequency.Weeks);

            var renewed = _fold.Renew(schedule, RenewalMode.Future, AsOfDate);

            renewed.NextDue.ShouldBe(new DateOnly(2025, 2, 27));
        }

        [Fact]
        public void Should_Not_Advance_Past_The_End_Date_In_Future_Mode()
        {
            var schedule = CreateSchedule(nextDue: "2025-02-20", endDate: "2025-02-25", frequency: Frequency.Weeks);

            var renewed = _fold.Renew(schedule, RenewalMode.Future, AsOfDate);

            renewed.ShouldBe(schedule, "the next occurrence would fall beyond the end date");
        }

        [Fact]
        public void Should_Catch_Up_Every_Overdue_Period()
        {
            // Weekly from 2025-01-01, evaluated on 2025-01-29: occurrences on the 1st, 8th, 15th, 22nd and 29th.
            var schedule = CreateSchedule(nextDue: "2025-01-01", frequency: Frequency.Weeks);

            var renewed = _fold.Renew(schedule, RenewalMode.Overdue, AsOfDate);

            renewed.NextDue.ShouldBe(new DateOnly(2025, 2, 5));
        }

        [Fact]
        public void Should_Advance_An_Income_Due_Today()
        {
            // RenewalMode.Overdue covers overdue and due-today items alike.
            var schedule = CreateSchedule(nextDue: "2025-01-29", frequency: Frequency.Weeks);

            var renewed = _fold.Renew(schedule, RenewalMode.Overdue, AsOfDate);

            renewed.NextDue.ShouldBe(new DateOnly(2025, 2, 5));
        }

        [Fact]
        public void Should_Stop_Catching_Up_At_The_End_Date()
        {
            var schedule = CreateSchedule(nextDue: "2025-01-01", endDate: "2025-01-15", frequency: Frequency.Weeks);

            var renewed = _fold.Renew(schedule, RenewalMode.Overdue, AsOfDate);

            renewed.NextDue.ShouldBe(new DateOnly(2025, 1, 15), "the schedule cannot advance beyond the end date");
        }
    }

    private static IncomeScheduleInput CreateSchedule(string nextDue, string? endDate = null, Frequency? frequency = null,
        int frequencyCount = 1, bool excludeFromCalcs = false)
    {
        return new IncomeScheduleInput
        {
            RowId = Guid.NewGuid(),
            NextDue = DateOnly.ParseExact(nextDue, "yyyy-MM-dd"),
            EndDate = endDate is null ? null : DateOnly.ParseExact(endDate, "yyyy-MM-dd"),
            Frequency = frequency ?? Frequency.Months,
            FrequencyCount = frequencyCount,
            ExcludeFromCalcs = excludeFromCalcs
        };
    }
}
