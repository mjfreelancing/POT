namespace Pot.App.Features.Accruals.Status.Models;

public sealed class Output
{
    public required IReadOnlyList<Guid> ExpenseRenewalsRequired { get; init; }
    public required IReadOnlyList<Guid> IncomeRenewalsRequired { get; init; }
}
