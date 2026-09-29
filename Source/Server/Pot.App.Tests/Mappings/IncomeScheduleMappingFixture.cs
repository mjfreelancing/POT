using Pot.App.Mappings;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Mappings;

public class IncomeScheduleMappingFixture : PotFixtureBase
{
    public class MapToScheduleInput : IncomeScheduleMappingFixture
    {
        private readonly AccountEntity _account;

        public MapToScheduleInput()
        {
            _account = EntityFactory.CreateAccount(EntityFactory.CreateSite(), "Test Account", 1000.0d);
        }

        [Fact]
        public void Should_Map_Every_Schedule_Fact_From_The_Income()
        {
            var income = EntityFactory.CreateIncome(_account, true, "Test Income", 250.0d, "2025-03-01", "2025-11-30",
                Frequency.Weeks, 2);

            var result = income.MapToScheduleInput();

            result.RowId.ShouldBe(income.RowId);
            result.ExcludeFromCalcs.ShouldBeTrue();
            result.NextDue.ShouldBe(new DateOnly(2025, 3, 1));
            result.EndDate.ShouldBe(new DateOnly(2025, 11, 30));
            result.Frequency.ShouldBe(Frequency.Weeks);
            result.FrequencyCount.ShouldBe(2);
        }

        [Fact]
        public void Should_Map_An_Income_That_Is_Included_In_Calcs()
        {
            var income = EntityFactory.CreateIncome(_account, false, "Included Income", 25.0d, "2025-01-15", null,
                Frequency.Months, 1);

            var result = income.MapToScheduleInput();

            result.ExcludeFromCalcs.ShouldBeFalse();
        }

        [Fact]
        public void Should_Map_Null_End_Date_When_Absent()
        {
            var income = EntityFactory.CreateIncome(_account, false, "Open Ended Income", 100.0d, "2025-04-01", null,
                Frequency.Months, 1);

            var result = income.MapToScheduleInput();

            result.EndDate.ShouldBeNull();
            result.NextDue.ShouldBe(new DateOnly(2025, 4, 1));
            result.Frequency.ShouldBe(Frequency.Months);
            result.FrequencyCount.ShouldBe(1);
        }

        [Fact]
        public void Should_Map_The_Persisted_Schedule_Rather_Than_The_Entity_Reference()
        {
            var income = EntityFactory.CreateIncome(_account, false, "Scheduled Income", 150.0d, "2025-05-01", "2025-10-01",
                Frequency.Months, 2);

            var result = income.MapToScheduleInput();

            // The projection is a snapshot: a later entity change must not be visible through the projected schedule.
            income.NextDue = new DateOnly(2025, 6, 1);
            income.Frequency = Frequency.Weeks;

            result.NextDue.ShouldBe(new DateOnly(2025, 5, 1));
            result.Frequency.ShouldBe(Frequency.Months);
        }
    }
}
