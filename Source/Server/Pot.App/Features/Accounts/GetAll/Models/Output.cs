namespace Pot.App.Features.Accounts.GetAll.Models;

public sealed class Output
{
    public required Guid RowId { get; init; }
    public required long Etag { get; init; }
    public required string Bsb { get; init; }
    public required string Number { get; init; }
    public required string Description { get; init; }
    public required double Balance { get; init; }
    public required double Reserved { get; init; }
    public required double TotalExpenseAccrued { get; init; }
    public required double DailyExpenseAccrual { get; init; }
    public required int LinkedExpenses { get; init; }
    public required int LinkedIncomes { get; init; }
    public double Available => Balance - Reserved - TotalExpenseAccrued;

}
