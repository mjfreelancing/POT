using Pot.App.Calculators;
using Pot.Shared.Enumerations;
using Pot.Shared.Models;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Calculators;

public class ExpenseRenewalFoldFixture : PotFixtureBase
{
    private static readonly DateOnly AsOfDate = new(2025, 1, 29);

    private readonly ExpenseRenewalFold _fold = new();

    public class Renew : ExpenseRenewalFoldFixture
    {
        [Fact]
        public void Should_Not_Advance_An_Excluded_Expense()
        {
            var facts = CreateExpense(nextDue: "2025-01-01", excludeFromCalcs: true);

            var renewed = Apply(facts, RenewalMode.Overdue, AsOfDate);

            renewed.ShouldBe(CreateCursor(facts));
        }

        [Fact]
        public void Should_Not_Advance_A_One_Time_Expense()
        {
            var facts = CreateExpense(nextDue: "2025-01-01", frequency: Frequency.OneTime, frequencyCount: 0);

            var renewed = Apply(facts, RenewalMode.Overdue, AsOfDate);

            renewed.ShouldBe(CreateCursor(facts));
        }

        [Fact]
        public void Should_Not_Advance_An_Expense_That_Has_Reached_Its_End_Date()
        {
            var facts = CreateExpense(nextDue: "2025-01-15", endDate: "2025-01-15");

            var renewed = Apply(facts, RenewalMode.Overdue, AsOfDate);

            renewed.ShouldBe(CreateCursor(facts));
        }

        [Fact]
        public void Should_Advance_Exactly_One_Period_In_Future_Mode()
        {
            var facts = CreateExpense(nextDue: "2025-02-20", accrualStart: "2025-01-01", frequency: Frequency.Weeks);

            var renewed = Apply(facts, RenewalMode.Future, AsOfDate);

            renewed.NextDue.ShouldBe(new DateOnly(2025, 2, 27));
            renewed.AccrualStart.ShouldBe(AsOfDate, "a future renewal restarts accrual from the renewal date");
        }

        [Fact]
        public void Should_Not_Advance_Past_The_End_Date_In_Future_Mode()
        {
            var facts = CreateExpense(nextDue: "2025-02-20", accrualStart: "2025-01-01", endDate: "2025-02-25",
                frequency: Frequency.Weeks);

            var renewed = Apply(facts, RenewalMode.Future, AsOfDate);

            renewed.ShouldBe(CreateCursor(facts), "the next occurrence would fall beyond the end date");
        }

        [Fact]
        public void Should_Catch_Up_Every_Overdue_Period()
        {
            // Weekly from 2025-01-01, evaluated on 2025-01-29: occurrences on the 1st, 8th, 15th, 22nd and 29th.
            var facts = CreateExpense(nextDue: "2025-01-01", accrualStart: "2025-01-01", frequency: Frequency.Weeks);

            var renewed = Apply(facts, RenewalMode.Overdue, AsOfDate);

            renewed.NextDue.ShouldBe(new DateOnly(2025, 2, 5));
            renewed.AccrualStart.ShouldBe(AsOfDate, "accrual restarts from the last occurrence settled");
        }

        [Fact]
        public void Should_Advance_An_Expense_Due_Today()
        {
            // RenewalMode.Overdue covers overdue and due-today items alike, which the projection's settlement
            // analysis depends on.
            var facts = CreateExpense(nextDue: "2025-01-29", accrualStart: "2025-01-22", frequency: Frequency.Weeks);

            var renewed = Apply(facts, RenewalMode.Overdue, AsOfDate);

            renewed.NextDue.ShouldBe(new DateOnly(2025, 2, 5));
            renewed.AccrualStart.ShouldBe(AsOfDate);
        }

        [Fact]
        public void Should_Stop_Catching_Up_At_The_End_Date()
        {
            var facts = CreateExpense(nextDue: "2025-01-01", accrualStart: "2025-01-01", endDate: "2025-01-15",
                frequency: Frequency.Weeks);

            var renewed = Apply(facts, RenewalMode.Overdue, AsOfDate);

            renewed.NextDue.ShouldBe(new DateOnly(2025, 1, 15), "the schedule cannot advance beyond the end date");
            renewed.AccrualStart.ShouldBe(new DateOnly(2025, 1, 8));
        }

        [Fact]
        public void Should_Clear_The_Accrual_Start_For_A_None_Policy_Expense()
        {
            var facts = CreateExpense(nextDue: "2025-01-01", accrualStart: "2025-01-01", accrualPolicy: AccrualPolicy.None,
                frequency: Frequency.Weeks);

            var renewed = Apply(facts, RenewalMode.Overdue, new DateOnly(2025, 1, 8));

            renewed.NextDue.ShouldBe(new DateOnly(2025, 1, 15));
            renewed.AccrualStart.ShouldBeNull();
        }
    }

    private AccrualCursor Apply(ExpenseAccrualInput facts, RenewalMode mode, DateOnly asOfDate)
    {
        return _fold.Renew(facts, CreateCursor(facts), mode, asOfDate);
    }

    private static AccrualCursor CreateCursor(ExpenseAccrualInput facts)
    {
        return new AccrualCursor(facts.NextDue, facts.AccrualStart);
    }

    private static ExpenseAccrualInput CreateExpense(string nextDue, string? accrualStart = "2025-01-01",
        string? endDate = null, Frequency? frequency = null, int frequencyCount = 1, AccrualPolicy? accrualPolicy = null,
        bool excludeFromCalcs = false)
    {
        return new ExpenseAccrualInput
        {
            RowId = Guid.NewGuid(),
            ExcludeFromCalcs = excludeFromCalcs,
            AccrualStart = accrualStart is null ? null : DateOnly.ParseExact(accrualStart, "yyyy-MM-dd"),
            NextDue = DateOnly.ParseExact(nextDue, "yyyy-MM-dd"),
            EndDate = endDate is null ? null : DateOnly.ParseExact(endDate, "yyyy-MM-dd"),
            AccrualPolicy = accrualPolicy ?? AccrualPolicy.Automatic,
            Frequency = frequency ?? Frequency.Months,
            FrequencyCount = frequencyCount,
            Amount = 100.0d
        };
    }
}
