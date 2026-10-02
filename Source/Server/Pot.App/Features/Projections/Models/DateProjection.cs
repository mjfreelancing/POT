namespace Pot.App.Features.Projections.Models;

/// <summary>
/// The projected values for a single account, or for the combined accounts, on a single forecast date.
/// </summary>
/// <remarks>
/// This is the published shape of a forecast day; the aggregates the day loop resolves internally are held in
/// <see cref="DateProjectionValues" />. The window assumes income is received and expenses are paid on time, so
/// <see cref="Balance" /> is simulated forward from the account's persisted balance rather than read from it.
/// The standing obligations a consumer may net out of it are published as <see cref="Reserved" />,
/// <see cref="UnpaidAccrual" /> and <see cref="Arrears" />.
/// </remarks>
public sealed class DateProjection
{
    /// <summary>The forecast date.</summary>
    public required DateOnly Date { get; init; }

    /// <summary>The balance at the end of the day, after the day's assumed cash movement.</summary>
    /// <remarks>
    /// This is the running balance carried forward, so it is the balance at the start of the day plus
    /// <see cref="IncomeReceived" /> less <see cref="ExpensesPaid" />.
    /// </remarks>
    public required double Balance { get; init; }

    /// <summary>The amount the account has set aside as reserved.</summary>
    public required double Reserved { get; init; }

    /// <summary>The past-due obligation measured on the read date and held for every day of the window.</summary>
    /// <remarks>
    /// Arrears is pre-existing debt the forecast never assumes is repaid, so it is never released.
    /// </remarks>
    public required double Arrears { get; init; }

    /// <summary>The accrual of the cycles in progress, net of the part the day's own payments settle.</summary>
    /// <remarks>
    /// Accrual is measured before the day's payments are applied, so an occurrence falling due today is counted in
    /// full, yet <see cref="Balance" /> already has that payment deducted. The part settled by the day's payments is
    /// therefore removed, limited to what the paid rows actually accrued, so a row that accrues nothing settles
    /// nothing.
    /// </remarks>
    public required double UnpaidAccrual { get; init; }

    /// <summary>The combined daily accrual rate for the account's cycles in progress on the day.</summary>
    public required double DailyAccrual { get; init; }

    /// <summary>The total income assumed received on the day.</summary>
    public required double IncomeReceived { get; init; }

    /// <summary>The total expenses assumed paid on the day.</summary>
    public required double ExpensesPaid { get; init; }

    /// <summary>The expenses due on the day, empty when the day has none.</summary>
    public required ProjectionExpenseModel[] ExpenseItems { get; init; }

    /// <summary>The income due on the day, empty when the day has none.</summary>
    public required ProjectionIncomeModel[] IncomeItems { get; init; }
}
