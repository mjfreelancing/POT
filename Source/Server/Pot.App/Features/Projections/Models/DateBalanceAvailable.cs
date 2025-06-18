namespace Pot.App.Features.Projections.Models;

public sealed class DateBalanceAvailable
{
    public required DateOnly Date { get; init; }
    public required double StartingBalance { get; set; }
    public required double IncomeReceived { get; set; }
    public required double ExpensesPaid { get; set; }
    public required double Accrued { get; set; }
    public required double Reserved { get; set; }

    public double Balance => StartingBalance + IncomeReceived - ExpensesPaid;
    public double Available => Balance - Reserved - Accrued;
}
