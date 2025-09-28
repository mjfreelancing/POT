namespace Pot.App.Features.Accruals.Status.Models;

public sealed class Output
{
    public required Guid[] ExpenseRenewalsRequired { get; init; }
    public required Guid[] ExpenseAccrualsRequired { get; init; }
    public required Guid[] IncomeRenewalsRequired { get; init; }
}
