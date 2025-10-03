namespace Pot.App.Features.Projections.Models;

internal sealed class DateProjectionValues
{
    public required DateOnly Date { get; init; }
    public required double StartingBalance { get; set; }
    public required double IncomeReceived { get; set; }
    public required double ExpensesPaid { get; set; }
    public required double DailyAccrual { get; set; }
    public required double Accrued { get; set; }
    public required double Reserved { get; set; }
    public required ProjectionExpenseModel[] ExpenseItems { get; init; }
    public required ProjectionIncomeModel[] IncomeItems { get; init; }
}
