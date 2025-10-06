
namespace Pot.App.Features.Maintenance.Import.Models;

public interface IAccountCsvRow
{
    double Balance { get; }
    string Bsb { get; }
    double DailyExpenseAccrual { get; }
    string Description { get; }
    string Number { get; }
    double Reserved { get; }
    Guid RowId { get; }
    double TotalExpenseAccrued { get; }
}