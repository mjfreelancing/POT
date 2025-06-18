namespace Pot.App.Features.Projections.Models;

public sealed class DateBalanceAvailable
{
    public required DateOnly Date { get; init; }
    public required double Balance { get; set; }
    public required double Accrued { get; set; }

    public double Available => Balance - Accrued;
}
