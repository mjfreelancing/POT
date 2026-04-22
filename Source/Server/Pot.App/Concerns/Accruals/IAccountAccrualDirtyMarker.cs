using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Concerns.Accruals;

public interface IAccountAccrualDirtyMarker : IPotScopedDependency
{
    Task MarkDirtyForCreateAsync(AccountEntity account, CancellationToken cancellationToken);
    Task MarkDirtyForToggleAsync(IReadOnlyCollection<ExpenseEntity> expenses, CancellationToken cancellationToken);
}
