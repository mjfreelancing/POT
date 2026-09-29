using AllOverIt.Extensions;
using Pot.App.Calculators;
using Pot.App.Mappings;
using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.Shared.Models;

namespace Pot.App.Features.Projections;

/// <summary>
/// The per-account state the projection day loop walks.
/// </summary>
/// <remarks>
/// A row is one persisted expense or income record, identified by its <c>RowId</c>, and the rows falling due on a
/// day are projected into the response as <see cref="Models.DateProjectionValues.ExpenseItems" /> and
/// <see cref="Models.DateProjectionValues.IncomeItems" />; <see cref="ExpenseProjectionRow" /> and
/// <see cref="IncomeProjectionRow" /> hold that per-record state.
///
/// Each row carries its immutable facts plus the schedule cursor the loop folds, so no entity is mutated by the
/// projection path. The running balance is held here rather than on the account, and the arrears as at today is held
/// for the whole window because it is pre-existing debt the forecast never assumes is repaid.
/// </remarks>
internal sealed class AccountProjectionState
{
    /// <summary>
    /// The loop state for one persisted expense.
    /// </summary>
    /// <remarks>
    /// The calculation works from <see cref="Facts" /> and <see cref="Cursor" /> and the projection
    /// path never mutates the entity it was projected from.
    /// </remarks>
    internal sealed class ExpenseProjectionRow
    {
        /// <summary>The expense entity the row was projected from.</summary>
        public ExpenseEntity Expense { get; }

        /// <summary>The immutable accrual facts projected from the entity.</summary>
        public ExpenseAccrualInput Facts { get; }

        /// <summary>The schedule position the loop measures the day against and folds forward as the window walks.</summary>
        public AccrualCursor Cursor { get; set; }

        /// <summary>
        /// Creates the row for an expense, seeding the cursor from the persisted schedule.
        /// </summary>
        /// <param name="expense">The expense to project.</param>
        public ExpenseProjectionRow(ExpenseEntity expense)
        {
            Expense = expense;
            Facts = expense.MapToAccrualInput();
            Cursor = new AccrualCursor(expense.NextDue, expense.AccrualStart);
        }
    }

    /// <summary>
    /// The loop state for one persisted income.
    /// </summary>
    /// <remarks>
    /// An income has no accrual state, so the row carries only the schedule facts the renewal fold advances.
    /// </remarks>
    internal sealed class IncomeProjectionRow
    {
        /// <summary>The income entity the row was projected from.</summary>
        public IncomeEntity Income { get; }

        /// <summary>The schedule facts, replaced with the folded schedule as the window walks.</summary>
        public IncomeScheduleInput Schedule { get; set; }

        /// <summary>
        /// Creates the row for an income, seeding the schedule from the persisted values.
        /// </summary>
        /// <param name="income">The income to project.</param>
        public IncomeProjectionRow(IncomeEntity income)
        {
            Income = income;
            Schedule = income.MapToScheduleInput();
        }
    }

    private readonly ExpenseProjectionRow[] _expenseRows;
    private readonly IncomeProjectionRow[] _incomeRows;
    private bool _arrearsHeld;

    /// <summary>The account the state was projected from.</summary>
    /// <remarks>
    /// The entity is only read; the loop keeps the state it moves forward on this instance, so the account row is
    /// never mutated by the projection path.
    /// </remarks>
    public AccountEntity Account { get; }

    /// <summary>The loop-local balance, seeded from the account balance and moved forward as the window walks.</summary>
    public double RunningBalance { get; private set; }

    /// <summary>The arrears as at today, reported unchanged for every day of the window.</summary>
    /// <remarks>
    /// Arrears is pre-existing debt the forecast never assumes is repaid, so it is held for the whole window rather
    /// than being released as the folded schedule advances.
    /// </remarks>
    public double Arrears { get; private set; }

    /// <summary>The expense rows, in the order the account's expenses were read.</summary>
    public ExpenseProjectionRow[] ExpenseRows => _expenseRows;

    /// <summary>The income rows, in the order the account's income was read.</summary>
    public IncomeProjectionRow[] IncomeRows => _incomeRows;

    /// <summary>
    /// Creates the state for an account, seeding the running balance from the account's balance and projecting a row
    /// for each of its expenses and income.
    /// </summary>
    /// <param name="account">The account to project.</param>
    public AccountProjectionState(AccountEntity account)
    {
        Account = account;
        RunningBalance = account.Balance;
        _expenseRows = account.Expenses.SelectToArray(expense => new ExpenseProjectionRow(expense));
        _incomeRows = account.Incomes.SelectToArray(income => new IncomeProjectionRow(income));
    }

    /// <summary>
    /// Positions every expense on its current cursor, so the calculation is measured at the start of the day.
    /// </summary>
    public IEnumerable<ExpenseAccrualPosition> CreatePositions()
    {
        return _expenseRows.Select(row => new ExpenseAccrualPosition(row.Facts, row.Cursor));
    }

    /// <summary>
    /// Holds the arrears as at today for the whole window.
    /// </summary>
    /// <param name="arrears">The arrears measured as at today.</param>
    /// <remarks>
    /// Later days report the same pre-existing debt, so only the first call has any effect. A later day's value is
    /// the folded schedule's own (zero) view and must not replace the held one, which is what releasing the
    /// obligation would look like.
    /// </remarks>
    public void HoldArrears(double arrears)
    {
        if (_arrearsHeld)
        {
            return;
        }

        Arrears = arrears;
        _arrearsHeld = true;
    }

    /// <summary>
    /// Applies the day's assumed cash movement to the loop-local running balance.
    /// </summary>
    /// <param name="incomeReceived">The income assumed received on the day.</param>
    /// <param name="expensesPaid">The expenses assumed paid on the day.</param>
    public void RecordCashMovement(double incomeReceived, double expensesPaid)
    {
        RunningBalance += incomeReceived - expensesPaid;
    }

    /// <summary>
    /// Folds the day's renewal for every row.
    /// </summary>
    /// <param name="expenseRenewalFold">The expense renewal fold.</param>
    /// <param name="incomeRenewalFold">The income renewal fold.</param>
    /// <param name="date">The day being folded.</param>
    /// <remarks>
    /// This runs after the day has been measured, so the due check, the paid set and the accrual all read the
    /// cursor as it stood at the start of the day.
    /// </remarks>
    public void FoldRenewals(IExpenseRenewalFold expenseRenewalFold, IIncomeRenewalFold incomeRenewalFold, DateOnly date)
    {
        foreach (var row in _expenseRows)
        {
            row.Cursor = expenseRenewalFold.Renew(row.Facts, row.Cursor, RenewalMode.Overdue, date);
        }

        foreach (var row in _incomeRows)
        {
            row.Schedule = incomeRenewalFold.Renew(row.Schedule, RenewalMode.Overdue, date);
        }
    }
}
