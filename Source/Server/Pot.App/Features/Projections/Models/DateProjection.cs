namespace Pot.App.Features.Projections.Models;

/// <summary>
/// The projected values for a single account, or for the combined accounts, on a single forecast date.
/// </summary>
/// <remarks>
/// This is the published shape of a forecast day; the aggregates the day loop resolves internally are held in
/// <see cref="DateProjectionValues" />. The window assumes income is received and expenses are paid on time, so
/// <see cref="Balance" /> and <see cref="Available" /> are simulated forward from the account's persisted balance
/// rather than read from it.
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

    /// <summary>The balance available once the account's standing obligations are deducted.</summary>
    /// <remarks>
    /// This is <see cref="Balance" /> less the reserved amount, less the accrual of the cycles in progress and the
    /// arrears carried from past-due cycles, plus the part of the day's accrual its own payments settle. Accrual is
    /// measured before the day's payments are applied, so an occurrence falling due today is counted in full; the
    /// add-back is limited to what the paid rows actually accrued, so a row that accrues nothing settles nothing.
    /// Arrears is pre-existing debt the forecast never assumes is repaid, so it is never released.
    /// </remarks>
    public required double Available { get; init; }

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
