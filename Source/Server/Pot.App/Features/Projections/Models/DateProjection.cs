namespace Pot.App.Features.Projections.Models;

public sealed class DateProjection
{
    public required DateOnly Date { get; init; }
    public required double Balance { get; init; }
    public required double Available { get; init; }
    public required double DailyAccrual { get; init; }
    public required double IncomeReceived { get; init; }
    public required double ExpensesPaid { get; init; }
}
