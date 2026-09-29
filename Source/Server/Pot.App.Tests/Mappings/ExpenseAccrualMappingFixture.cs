using Pot.App.Mappings;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Mappings;

public class ExpenseAccrualMappingFixture : PotFixtureBase
{
    public class MapToAccrualInput : ExpenseAccrualMappingFixture
    {
        private readonly AccountEntity _account;

        public MapToAccrualInput()
        {
            _account = EntityFactory.CreateAccount(EntityFactory.CreateSite(), "Test Account", 1000.0d);
        }

        [Fact]
        public void Should_Map_Every_Accrual_Fact_From_The_Expense()
        {
            var expense = EntityFactory.CreateExpense(_account, true, "Test Expense", 123.45d, "2025-01-01", "2025-02-01",
                "2025-12-31", Frequency.Months, 3, AccrualPolicy.Automatic);

            var result = expense.MapToAccrualInput();

            result.RowId.ShouldBe(expense.RowId);
            result.ExcludeFromCalcs.ShouldBeTrue();
            result.AccrualStart.ShouldBe(new DateOnly(2025, 1, 1));
            result.NextDue.ShouldBe(new DateOnly(2025, 2, 1));
            result.EndDate.ShouldBe(new DateOnly(2025, 12, 31));
            result.AccrualPolicy.ShouldBe(AccrualPolicy.Automatic);
            result.Frequency.ShouldBe(Frequency.Months);
            result.FrequencyCount.ShouldBe(3);
            result.Amount.ShouldBe(123.45d);
        }

        [Fact]
        public void Should_Map_An_Expense_That_Is_Included_In_Calcs()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Included Expense", 25.0d, "2025-01-01", "2025-01-15",
                null, Frequency.OneTime, 1, AccrualPolicy.None);

            var result = expense.MapToAccrualInput();

            result.ExcludeFromCalcs.ShouldBeFalse();
        }

        [Fact]
        public void Should_Map_Null_Accrual_Start_And_End_Date_When_Absent()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Open Ended Expense", 50.0d, "2025-01-01", "2025-01-15",
                null, Frequency.Weeks, 2);
            expense.AccrualStart = null;

            var result = expense.MapToAccrualInput();

            result.AccrualStart.ShouldBeNull();
            result.EndDate.ShouldBeNull();
            result.NextDue.ShouldBe(new DateOnly(2025, 1, 15));
            result.Frequency.ShouldBe(Frequency.Weeks);
            result.FrequencyCount.ShouldBe(2);
            result.Amount.ShouldBe(50.0d);
        }

        [Fact]
        public void Should_Map_The_Persisted_Schedule_Rather_Than_The_Entity_Reference()
        {
            var expense = EntityFactory.CreateExpense(_account, false, "Scheduled Expense", 75.0d, "2025-01-01", "2025-02-01",
                "2025-12-31", Frequency.Months, 1, AccrualPolicy.Automatic);

            var result = expense.MapToAccrualInput();

            // The projection is a snapshot: a later entity change must not be visible through the projected facts.
            expense.NextDue = new DateOnly(2025, 3, 1);
            expense.Amount = 999.0d;

            result.NextDue.ShouldBe(new DateOnly(2025, 2, 1));
            result.Amount.ShouldBe(75.0d);
        }
    }
}
