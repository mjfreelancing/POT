using Pot.App.Calculators;
using Pot.Shared.Enumerations;
using Pot.Shared.Models;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Calculators;

public class AccrualCalculatorFixture : PotFixtureBase
{
    // The settlement fixtures share one bill: 100 on a 31-day cycle, measured on 2025-01-15. The occurrence
    // before it is 2025-01-14 and the next cycle falls due on 2025-02-15.
    protected const double BillAmount = 100.0d;
    protected static readonly DateOnly AsOfDate = new(2025, 1, 15);
    protected static readonly DateOnly DayBeforeAsOfDate = new(2025, 1, 14);
    protected static readonly DateOnly NextCycleDue = new(2025, 2, 15);

    protected IAccrualCalculator Calculator { get; } = new AccrualCalculator();

    protected static ExpenseAccrualInput CreateExpense(double amount = BillAmount, string nextDue = "2025-01-15",
        string? accrualStart = "2025-01-15", string? endDate = null, Frequency? frequency = null, int frequencyCount = 1,
        AccrualPolicy? accrualPolicy = null, bool excludeFromCalcs = false, Guid? rowId = null)
    {
        var resolvedFrequency = frequency ?? Frequency.Months;

        return new ExpenseAccrualInput
        {
            RowId = rowId ?? Guid.NewGuid(),
            ExcludeFromCalcs = excludeFromCalcs,
            AccrualStart = ParseDate(accrualStart),
            NextDue = ParseDate(nextDue)!.Value,
            EndDate = ParseDate(endDate),
            AccrualPolicy = accrualPolicy ?? AccrualPolicy.Automatic,
            Frequency = resolvedFrequency,
            FrequencyCount = frequencyCount,
            Amount = amount
        };
    }

    /// <summary>Positions an expense on the schedule it was persisted with.</summary>
    protected static ExpenseAccrualPosition CreatePosition(ExpenseAccrualInput facts)
    {
        return new ExpenseAccrualPosition(facts, new AccrualCursor(facts.NextDue, facts.AccrualStart));
    }

    /// <summary>Positions an expense at a folded cursor, as the projection loop does as it walks days.</summary>
    protected static ExpenseAccrualPosition CreatePosition(ExpenseAccrualInput facts, string nextDue, string? accrualStart)
    {
        return new ExpenseAccrualPosition(facts, new AccrualCursor(ParseDate(nextDue)!.Value, ParseDate(accrualStart)));
    }

    protected AccountAccrualView Calculate(ExpenseAccrualPosition position, DateOnly asOfDate)
    {
        return Calculator.CalculateAccountWithExpenseDetail([position], asOfDate);
    }

    /// <summary>
    /// The accounts form of available funds: balance less reserved less the two accrual obligations (reserved is 0).
    /// </summary>
    protected static double CalculateAvailable(double balance, AccountAccrualView view)
    {
        return balance - view.TotalExpenseAccrued - view.TotalArrears;
    }

    private static DateOnly? ParseDate(string? value)
    {
        return value is null ? null : DateOnly.ParseExact(value, "yyyy-MM-dd");
    }

    public class CalculateAccountTotals : AccrualCalculatorFixture
    {
        [Fact]
        public void Should_Throw_When_Positions_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = Calculator.CalculateAccountTotals(null!, AsOfDate);
            });

            exception.ParamName.ShouldBe("positions");
        }

        [Fact]
        public void Should_Return_A_Zeroed_View_When_There_Are_No_Expenses()
        {
            var view = Calculator.CalculateAccountTotals([], AsOfDate);

            view.TotalExpenseAccrued.ShouldBe(0.0d);
            view.TotalArrears.ShouldBe(0.0d);
            view.TotalCommitted.ShouldBe(0.0d);
            view.DailyExpenseAccrual.ShouldBe(0.0d);
            view.StableExpenseAccrual.ShouldBe(0.0d);
            view.Expenses.ShouldBeEmpty();
        }

        [Fact]
        public void Should_Sum_Every_Expense()
        {
            var paidToday = CreateExpense(nextDue: "2025-01-15", accrualStart: "2025-01-15");
            var nonePolicyPastDue = CreateExpense(nextDue: "2025-01-01", accrualStart: null, accrualPolicy: AccrualPolicy.None);
            var excludedPastDue = CreateExpense(nextDue: "2025-01-01", accrualStart: "2025-01-01", excludeFromCalcs: true);
            var positions = new[]
            {
                CreatePosition(paidToday),
                CreatePosition(nonePolicyPastDue),
                CreatePosition(excludedPastDue)
            };

            var view = Calculator.CalculateAccountTotals(positions, AsOfDate);

            // Only the row due today accrues, so the excluded row and the None-policy row contribute nothing to it.
            view.TotalExpenseAccrued.ShouldBe(BillAmount, "only the due-today row accrues");

            // The None-policy row still carries its past-due obligation; the excluded row is absent entirely.
            view.TotalArrears.ShouldBe(BillAmount, "the None-policy row carries arrears while the excluded row does not");
            view.TotalCommitted.ShouldBe(200.0d, 0.001d);
        }

        [Fact]
        public void Should_Expose_TotalCommitted_As_Accrued_Plus_Arrears()
        {
            var pastDue = CreateExpense(nextDue: "2025-01-14", accrualStart: "2025-01-14");

            var view = Calculator.CalculateAccountTotals([CreatePosition(pastDue)], AsOfDate);

            view.TotalExpenseAccrued.ShouldBe(3.23d);
            view.TotalArrears.ShouldBe(BillAmount);
            view.TotalCommitted.ShouldBe(view.TotalExpenseAccrued + view.TotalArrears);
        }

        [Fact]
        public void Should_Not_Include_Per_Expense_Detail()
        {
            var expense = CreateExpense();

            var view = Calculator.CalculateAccountTotals([CreatePosition(expense)], AsOfDate);

            view.Expenses.ShouldBeEmpty();
        }
    }

    public class CalculateAccountWithExpenseDetail : AccrualCalculatorFixture
    {
        [Fact]
        public void Should_Throw_When_Positions_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = Calculator.CalculateAccountWithExpenseDetail(null!, AsOfDate);
            });

            exception.ParamName.ShouldBe("positions");
        }

        [Fact]
        public void Should_Return_A_Result_For_Every_Row_In_The_Supplied_Order()
        {
            var firstRowId = Guid.NewGuid();
            var secondRowId = Guid.NewGuid();
            var thirdRowId = Guid.NewGuid();
            var positions = new[]
            {
                CreatePosition(CreateExpense(rowId: firstRowId)),
                CreatePosition(CreateExpense(rowId: secondRowId)),
                CreatePosition(CreateExpense(rowId: thirdRowId))
            };

            var view = Calculator.CalculateAccountWithExpenseDetail(positions, AsOfDate);

            view.Expenses.Count.ShouldBe(3, "the list renders a value for every row of the account");
            view.Expenses.Select(detail => detail.RowId).ShouldBe([firstRowId, secondRowId, thirdRowId]);
        }

        [Fact]
        public void Should_Report_Zero_For_An_Excluded_Row()
        {
            var excluded = CreateExpense(nextDue: "2025-01-01", accrualStart: "2025-01-01", excludeFromCalcs: true);

            var view = Calculate(CreatePosition(excluded), AsOfDate);

            var detail = view.Expenses.ShouldHaveSingleItem();
            detail.Accrued.ShouldBe(0.0d);
            detail.Arrears.ShouldBe(0.0d, "an excluded row is the opt-out from every accrual figure");
        }

        [Fact]
        public void Should_Report_Zero_Accrued_But_Keep_Arrears_For_A_None_Policy_Row()
        {
            var nonePolicy = CreateExpense(nextDue: "2025-01-01", accrualStart: null, accrualPolicy: AccrualPolicy.None);

            var view = Calculate(CreatePosition(nonePolicy), AsOfDate);

            var detail = view.Expenses.ShouldHaveSingleItem();
            detail.Accrued.ShouldBe(0.0d);
            detail.Arrears.ShouldBe(BillAmount, "being behind on a bill is not the same question as whether it accrues");
        }

        [Fact]
        public void Should_Report_Zero_Accrued_But_Keep_Arrears_For_A_Row_Without_An_Accrual_Start()
        {
            var withoutStart = CreateExpense(nextDue: "2025-01-01", accrualStart: null);

            var view = Calculate(CreatePosition(withoutStart), AsOfDate);

            var detail = view.Expenses.ShouldHaveSingleItem();
            detail.Accrued.ShouldBe(0.0d, "a null accrual start is not an accrual start of zero");
            detail.Arrears.ShouldBe(BillAmount);
        }
    }

    public class SettlementStates : AccrualCalculatorFixture
    {
        [Fact]
        public void Should_Leave_A_Row_Settled_A_Day_Early_Unchanged()
        {
            // Settled the day before the as-of date, so the cursor is already advanced and the row ramps towards 2025-02-15.
            var settled = CreateExpense(nextDue: "2025-02-15", accrualStart: "2025-01-14");

            var view = Calculate(CreatePosition(settled), AsOfDate);

            // 100 / 32 = 3.125, rounded away from zero, which also pins the midpoint rounding mode.
            view.TotalExpenseAccrued.ShouldBe(3.13d);
            view.TotalArrears.ShouldBe(0.0d);
            CalculateAvailable(1000.0d, view).ShouldBe(996.87d, 0.001d);
        }

        [Fact]
        public void Should_Leave_A_Row_Settled_On_Its_Due_Date_Unchanged()
        {
            // Settled on the as-of date, so no days have elapsed in the cycle in progress.
            var settled = CreateExpense(nextDue: "2025-02-15", accrualStart: "2025-01-15");

            var view = Calculate(CreatePosition(settled), AsOfDate);

            view.TotalExpenseAccrued.ShouldBe(0.0d);
            view.TotalArrears.ShouldBe(0.0d);
            CalculateAvailable(1000.0d, view).ShouldBe(1000.0d, 0.001d);
        }

        [Fact]
        public void Should_Accrue_A_Due_Today_Row_In_Full_Without_Exceeding_The_Balance()
        {
            // The payment-day defect: the row is assumed unpaid, so it accrues in full. Measuring the
            // cycle in progress rather than the next period is what stops available exceeding balance.
            var dueToday = CreateExpense(nextDue: "2025-01-15", accrualStart: "2025-01-15");

            var view = Calculate(CreatePosition(dueToday), AsOfDate);

            view.TotalExpenseAccrued.ShouldBe(BillAmount);
            view.TotalArrears.ShouldBe(0.0d, "a bill due today is the current bill and is not arrears");
            view.DailyExpenseAccrual.ShouldBe(3.2258d, 0.001d, "the full amount spread over the cycle now starting");

            var available = CalculateAvailable(1000.0d, view);
            available.ShouldBe(900.0d, 0.001d);
            available.ShouldBeLessThanOrEqualTo(1000.0d);
        }

        [Fact]
        public void Should_Carry_A_Past_Due_Row_As_Arrears_Beside_The_Cycle_In_Progress()
        {
            // The occurrence due on 2025-01-14 is un-settled, so it is carried as arrears (100) while the cycle
            // in progress ramps from the occurrence that was just skipped - one day of 100/31.
            var pastDue = CreateExpense(nextDue: "2025-01-14", accrualStart: "2025-01-14");

            var view = Calculate(CreatePosition(pastDue), AsOfDate);

            view.TotalExpenseAccrued.ShouldBe(3.23d);
            view.TotalArrears.ShouldBe(BillAmount);

            // carrying the past-due occurrence as arrears leaves the as-of-date rate reporting the cycle in
            // progress, which is the same rate the existing rules produce.
            view.DailyExpenseAccrual.ShouldBe(3.2258d, 0.001d, "the remaining obligation over the days until the next cycle");
            CalculateAvailable(1000.0d, view).ShouldBe(896.77d, 0.001d);
        }

        [Fact]
        public void Should_Not_Exceed_The_Balance_For_A_None_Policy_Row_Due_Today()
        {
            // a non-accruing row never accrues, so the add-back cannot inflate available.
            var nonePolicy = CreateExpense(nextDue: "2025-01-15", accrualStart: "2025-01-15", accrualPolicy: AccrualPolicy.None);

            var view = Calculate(CreatePosition(nonePolicy), AsOfDate);

            view.TotalExpenseAccrued.ShouldBe(0.0d);
            view.TotalArrears.ShouldBe(0.0d);
            CalculateAvailable(1000.0d, view).ShouldBe(1000.0d);
        }

        [Fact]
        public void Should_Not_Exceed_The_Balance_For_A_Row_Without_An_Accrual_Start_Due_Today()
        {
            var withoutStart = CreateExpense(nextDue: "2025-01-15", accrualStart: null);

            var view = Calculate(CreatePosition(withoutStart), AsOfDate);

            view.TotalExpenseAccrued.ShouldBe(0.0d);
            view.TotalArrears.ShouldBe(0.0d);
            CalculateAvailable(1000.0d, view).ShouldBe(1000.0d);
        }

        [Fact]
        public void Should_Not_Mask_A_Temporary_Overdraft_On_The_Day_It_Exists()
        {
            var dueToday = CreateExpense(nextDue: "2025-01-15", accrualStart: "2025-01-15");

            var view = Calculate(CreatePosition(dueToday), AsOfDate);

            CalculateAvailable(0.0d, view).ShouldBe(-100.0d, 0.001d, "a zero balance with a bill due today is overdrawn");
        }

        [Fact]
        public void Should_Measure_A_Mid_Window_Payment_Day_Like_The_As_Of_Date()
        {
            // The ordering rule applies to every day of the window, not to the as-of date alone: a bill due part way
            // through a forecast accrues its full amount on its own due day rather than the next period's ramp.
            var midWindowBill = CreateExpense(nextDue: "2025-06-15", accrualStart: "2025-05-15");

            var view = Calculator.CalculateAccountWithExpenseDetail([CreatePosition(midWindowBill)], new DateOnly(2025, 6, 15));

            view.TotalExpenseAccrued.ShouldBe(BillAmount, "the cycle in progress is due today, wherever today falls");
            view.TotalArrears.ShouldBe(0.0d);
        }
    }

    public class Arrears : AccrualCalculatorFixture
    {
        [Fact]
        public void Should_Carry_One_Amount_For_Every_Missed_Cycle()
        {
            // 3.2 worked example: a 70 weekly bill at 10/day, two cycles missed and three days into the third.
            var weeklyBill = CreateExpense(amount: 70.0d, nextDue: "2025-01-01", accrualStart: "2025-01-01",
                frequency: Frequency.Weeks);

            var view = Calculator.CalculateAccountWithExpenseDetail([CreatePosition(weeklyBill)], new DateOnly(2025, 1, 11));

            view.TotalArrears.ShouldBe(140.0d, "two occurrences due on 2025-01-01 and 2025-01-08 have passed un-settled");
            view.TotalExpenseAccrued.ShouldBe(30.0d, "the cycle due on 2025-01-15 is three days in at 10/day");
            view.TotalCommitted.ShouldBe(170.0d, 0.001d);
            view.DailyExpenseAccrual.ShouldBe(10.0d, 0.001d);
        }

        [Fact]
        public void Should_Not_Treat_An_Occurrence_Due_Today_As_Arrears()
        {
            var weeklyBill = CreateExpense(amount: 70.0d, nextDue: "2025-01-11", accrualStart: "2025-01-11",
                frequency: Frequency.Weeks);

            var view = Calculator.CalculateAccountWithExpenseDetail([CreatePosition(weeklyBill)], new DateOnly(2025, 1, 11));

            view.TotalArrears.ShouldBe(0.0d, "the boundary stops a due-today bill being counted twice");
            view.TotalExpenseAccrued.ShouldBe(70.0d);
        }

        [Fact]
        public void Should_Stop_Counting_Arrears_At_The_End_Date()
        {
            // Occurrences fall on 2025-01-01 and 2025-01-08, both within the end date; the next one (2025-01-15)
            // is beyond it and is neither counted as arrears nor accrued.
            var endedBill = CreateExpense(amount: 70.0d, nextDue: "2025-01-01", accrualStart: "2025-01-01",
                endDate: "2025-01-08", frequency: Frequency.Weeks);

            var view = Calculator.CalculateAccountWithExpenseDetail([CreatePosition(endedBill)], new DateOnly(2025, 1, 11));

            view.TotalArrears.ShouldBe(140.0d);
            view.TotalExpenseAccrued.ShouldBe(0.0d, "nothing is left to accrue towards once the schedule has ended");
            view.DailyExpenseAccrual.ShouldBe(0.0d);
            view.StableExpenseAccrual.ShouldBe(0.0d);
        }

        [Fact]
        public void Should_Carry_At_Most_One_Occurrence_For_A_One_Time_Expense()
        {
            var oneTime = CreateExpense(nextDue: "2025-01-01", accrualStart: null, frequency: Frequency.OneTime,
                frequencyCount: 0, accrualPolicy: AccrualPolicy.None);

            // A one-time expense does not renew, so however long it has been outstanding it stays a single obligation.
            var view = Calculator.CalculateAccountWithExpenseDetail([CreatePosition(oneTime)], new DateOnly(2025, 6, 15));

            view.TotalArrears.ShouldBe(BillAmount);
            view.TotalExpenseAccrued.ShouldBe(0.0d);
        }

        [Fact]
        public void Should_Not_Count_An_Occurrence_Beyond_The_End_Date()
        {
            // 3.2: arrears stops at the end date, so an occurrence after it is neither counted nor accrued towards.
            // Create and update validation keep the due date within the end date, so this is defensive rather than
            // ordinary data; the boundary is asserted so it cannot move silently.
            var recurring = CreateExpense(nextDue: "2025-01-20", accrualStart: "2025-01-01", endDate: "2025-01-10");
            var oneTime = CreateExpense(nextDue: "2025-01-20", accrualStart: null, endDate: "2025-01-10",
                frequency: Frequency.OneTime, frequencyCount: 0, accrualPolicy: AccrualPolicy.None);

            var view = Calculator.CalculateAccountWithExpenseDetail([CreatePosition(recurring), CreatePosition(oneTime)],
                new DateOnly(2025, 1, 25));

            view.TotalArrears.ShouldBe(0.0d, "neither the walked schedule nor the single one-time occurrence is counted");
            view.TotalExpenseAccrued.ShouldBe(0.0d);
        }

        [Fact]
        public void Should_Stop_Walking_A_Schedule_That_Cannot_Advance()
        {
            // A zero frequency count cannot move the schedule on. The walk has to terminate rather than loop, and
            // the occurrence it stalled on is still owed.
            var malformed = CreateExpense(nextDue: "2025-01-01", accrualStart: "2025-01-01", frequency: Frequency.Days,
                frequencyCount: 0);

            var view = Calculator.CalculateAccountWithExpenseDetail([CreatePosition(malformed)], AsOfDate);

            view.TotalArrears.ShouldBe(BillAmount);
            view.TotalExpenseAccrued.ShouldBe(0.0d);
        }
    }

    public class AccrualRules : AccrualCalculatorFixture
    {
        [Fact]
        public void Should_Short_Circuit_For_A_None_Policy_Row()
        {
            var nonePolicy = CreateExpense(accrualPolicy: AccrualPolicy.None);

            var view = Calculate(CreatePosition(nonePolicy), AsOfDate);

            view.TotalExpenseAccrued.ShouldBe(0.0d);
            view.TotalArrears.ShouldBe(0.0d);
            view.DailyExpenseAccrual.ShouldBe(0.0d);
            view.StableExpenseAccrual.ShouldBe(0.0d);
        }

        [Fact]
        public void Should_Not_Accrue_Before_The_Accrual_Start()
        {
            var notStarted = CreateExpense(nextDue: "2025-02-15", accrualStart: "2025-02-01");

            var view = Calculate(CreatePosition(notStarted), AsOfDate);

            view.TotalExpenseAccrued.ShouldBe(0.0d);
            view.DailyExpenseAccrual.ShouldBe(0.0d);
            view.StableExpenseAccrual.ShouldBe(0.0d);
        }

        [Fact]
        public void Should_Round_The_Ramp_To_Two_Decimals()
        {
            // 100 over 30 days, 14 days elapsed: 46.6666... rounds to 46.67.
            var ramping = CreateExpense(nextDue: "2025-01-31", accrualStart: "2025-01-01");

            var view = Calculate(CreatePosition(ramping), AsOfDate);

            view.TotalExpenseAccrued.ShouldBe(46.67d);
        }

        [Fact]
        public void Should_Not_Accrue_Daily_For_A_One_Time_Expense_On_Its_Due_Date()
        {
            var oneTime = CreateExpense(nextDue: "2025-01-15", accrualStart: "2025-01-15",
                frequency: Frequency.OneTime, frequencyCount: 0);

            var view = Calculate(CreatePosition(oneTime), AsOfDate);

            view.TotalExpenseAccrued.ShouldBe(BillAmount);
            view.DailyExpenseAccrual.ShouldBe(0.0d, "a one-time expense does not accrue beyond its occurrence");
        }

        [Fact]
        public void Should_Use_The_Fixed_Denominator_For_A_One_Time_Stable_Contribution()
        {
            // NextDue less AccrualStart is 20 days, so the contribution is 100/20 however close the due date is.
            var oneTime = CreateExpense(nextDue: "2025-01-21", accrualStart: "2025-01-01",
                frequency: Frequency.OneTime, frequencyCount: 0);

            var view = Calculate(CreatePosition(oneTime), AsOfDate);

            view.StableExpenseAccrual.ShouldBe(5.0d);
        }

        [Fact]
        public void Should_Not_Contribute_Stable_On_Or_After_A_One_Time_Due_Date()
        {
            var oneTime = CreateExpense(nextDue: "2025-01-15", accrualStart: "2025-01-01",
                frequency: Frequency.OneTime, frequencyCount: 0);

            var view = Calculate(CreatePosition(oneTime), AsOfDate);

            view.StableExpenseAccrual.ShouldBe(0.0d);
        }

        [Fact]
        public void Should_Contribute_Stable_For_A_Recurring_Expense()
        {
            var recurring = CreateExpense(nextDue: "2025-02-15", accrualStart: "2025-01-15");

            var view = Calculate(CreatePosition(recurring), AsOfDate);

            // 100 divided by the calendar-averaged month length of 365.2425/12 days.
            view.StableExpenseAccrual.ShouldBe(3.2855d, 0.001d);
        }

        [Fact]
        public void Should_Contribute_Stable_On_The_End_Date()
        {
            var recurring = CreateExpense(nextDue: "2025-02-15", accrualStart: "2025-01-15", endDate: "2025-01-15");

            var view = Calculate(CreatePosition(recurring), AsOfDate);

            view.StableExpenseAccrual.ShouldBe(3.2855d, 0.001d, "the end-date boundary is inclusive");
        }

        [Fact]
        public void Should_Stop_Stable_Contributions_After_The_End_Date()
        {
            var recurring = CreateExpense(nextDue: "2025-02-15", accrualStart: "2025-01-15", endDate: "2025-01-14");

            var view = Calculate(CreatePosition(recurring), AsOfDate);

            view.StableExpenseAccrual.ShouldBe(0.0d);
        }

        [Fact]
        public void Should_Spread_The_Remaining_Obligation_Over_The_Days_Until_The_Cycle_Falls_Due()
        {
            // A row settled a day early: 3.13 has accrued of the 100, leaving 96.87 over the 31 days to 2025-02-15.
            var settled = CreateExpense(nextDue: "2025-02-15", accrualStart: "2025-01-14");

            var view = Calculate(CreatePosition(settled), AsOfDate);

            view.DailyExpenseAccrual.ShouldBe(3.1258d, 0.001d);
        }

        [Fact]
        public void Should_Not_Contribute_Stable_For_A_Recurring_Expense_At_Another_Frequency()
        {
            // Each frequency reaches its own average period, so pin one value per branch of the frequency helper.
            // EndOfMonth shares the calendar-averaged month length used by Months.
            var expectations = new Dictionary<Frequency, double>
            {
                [Frequency.Days] = 100.0d,
                [Frequency.Weeks] = 14.2857d,
                [Frequency.Months] = 3.2855d,
                [Frequency.EndOfMonth] = 3.2855d,
                [Frequency.Years] = 0.2738d
            };

            foreach (var (frequency, expected) in expectations)
            {
                var recurring = CreateExpense(nextDue: "2025-02-15", accrualStart: "2025-01-15", frequency: frequency);

                var view = Calculate(CreatePosition(recurring), AsOfDate);

                view.StableExpenseAccrual.ShouldBe(expected, 0.0001d, $"stable contribution for {frequency.Name}");
            }
        }

        [Fact]
        public void Should_Not_Accrue_Daily_For_A_Recurring_Expense_With_No_Further_Occurrence()
        {
            // Due today, but the next occurrence (2025-02-15) falls after the end date, so there is no cycle to
            // accrue towards and the daily rate is zero.
            var endingBill = CreateExpense(nextDue: "2025-01-15", accrualStart: "2025-01-15", endDate: "2025-02-01");

            var view = Calculate(CreatePosition(endingBill), AsOfDate);

            view.TotalExpenseAccrued.ShouldBe(BillAmount);
            view.DailyExpenseAccrual.ShouldBe(0.0d);
        }

        [Fact]
        public void Should_Bound_The_Accrual_Of_The_Cycle_In_Progress_By_The_Amount()
        {
            var weeklyBill = CreateExpense(amount: 70.0d, nextDue: "2025-01-01", accrualStart: "2025-01-01",
                frequency: Frequency.Weeks);

            var view = Calculator.CalculateAccountWithExpenseDetail([CreatePosition(weeklyBill)], new DateOnly(2025, 1, 11));

            view.TotalExpenseAccrued.ShouldBeLessThanOrEqualTo(70.0d);
        }
    }
}
