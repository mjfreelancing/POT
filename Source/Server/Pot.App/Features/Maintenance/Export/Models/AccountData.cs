namespace Pot.App.Features.Maintenance.Export.Models;

public sealed class AccountData
{
    public required Guid RowId { get; set; }
    public required string Bsb { get; set; }
    public required string Number { get; set; }
    public required string Description { get; set; }
    public double Balance { get; set; }
    public double Reserved { get; set; }
    public double TotalExpenseAccrued { get; set; }
    public double DailyExpenseAccrual { get; set; }
}