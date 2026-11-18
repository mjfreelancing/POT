using Pot.App.Concerns.Money;

namespace Pot.App.Features.Projections.Models;

/// <summary>
/// The per-account values the projection day loop resolves for a single date.
/// </summary>
/// <remarks>
/// This is internal loop state rather than a response contract. <see cref="DateProjection" /> is the shape the API
/// exposes, and the aggregates held here but not published (for example <see cref="StartingBalance" />,
/// <see cref="Accrued" />, <see cref="Arrears" /> and <see cref="Reserved" />) exist so the available balance for
/// the date can be derived.
/// </remarks>
internal sealed class DateProjectionValues
{
    /// <summary>The date the values are resolved for.</summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// The running balance at the start of the day, before the day's assumed cash movement.
    /// </summary>
    /// <remarks>
    /// The balance is loop-local: the window assumes income is received and expenses are paid on time, so the
    /// forecast simulates movement forward.
    /// </remarks>
    public required double StartingBalance { get; init; }

    /// <summary>The total income assumed received on the day.</summary>
    public required double IncomeReceived { get; init; }

    /// <summary>The total expenses assumed paid on the day.</summary>
    public required double ExpensesPaid { get; init; }

    /// <summary>
    /// The part of <see cref="Accrued" /> that the day's payments settle.
    /// </summary>
    /// <remarks>
    /// <see cref="Accrued" /> is measured before the day's payments are applied, so the occurrence falling due today
    /// is still counted in full. This is the sum of the per-expense accruals of the expense rows due on the day (the
    /// same rows listed in <see cref="ExpenseItems" />), matched by <see cref="ProjectionExpenseModel.RowId" />, and
    /// the response mapping adds it back so a payment day is not charged twice. Only what a row actually
    /// contributed to <see cref="Accrued" /> is settled, so an expense that accrues nothing settles nothing.
    /// </remarks>
    public required double AccrualSettledByPayments { get; init; }

    /// <summary>The combined daily accrual rate for the account's cycles in progress.</summary>
    public required double DailyAccrual { get; init; }

    /// <summary>The total accrual of the account's cycles in progress as at the day.</summary>
    public required double Accrued { get; init; }

    /// <summary>The total obligation carried for occurrences already past due.</summary>
    /// <remarks>
    /// The value is measured when the projection is calculated and held for the whole window because it is
    /// pre-existing debt the forecast never assumes is repaid, so it is subtracted from the available balance and
    /// never released.
    /// </remarks>
    public required double Arrears { get; init; }

    /// <summary>The amount reserved on the account, subtracted from the available balance.</summary>
    public required double Reserved { get; init; }

    /// <summary>The expenses due on the day.</summary>
    public required IReadOnlyList<ProjectionExpenseModel> ExpenseItems { get; init; }

    /// <summary>The incomes due on the day.</summary>
    public required IReadOnlyList<ProjectionIncomeModel> IncomeItems { get; init; }

    /// <summary>
    /// The balance the day closes on: the balance it started with, plus the income assumed received, less the
    /// expenses assumed paid.
    /// </summary>
    /// <remarks>
    /// This is the balance the day publishes and the balance the next day starts from, so a later day is measured
    /// from the figure a reader was shown rather than a recomputation of it.
    /// </remarks>
    public double Balance => MonetaryRounding.Round(StartingBalance + IncomeReceived - ExpensesPaid);

    /// <summary>
    /// The part of <see cref="Accrued" /> that the day's payments do not settle.
    /// </summary>
    /// <remarks>
    /// Expenses due on the day are published as paid, so the balance already has them deducted. Only the accrual the
    /// day's payments do not cover is published here, so a consumer netting it from the balance never deducts the
    /// same bill twice.
    /// </remarks>
    public double UnpaidAccrual => MonetaryRounding.Round(Accrued - AccrualSettledByPayments);
}
