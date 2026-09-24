
namespace Pot.App.Features.Maintenance.Import.Models;

public interface IAccountCsvRow
{
    double Balance { get; }
    double DailyExpenseAccrual { get; }
    string Description { get; }
    double Reserved { get; }
    Guid RowId { get; }
    double TotalExpenseAccrued { get; }
}