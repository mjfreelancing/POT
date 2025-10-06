using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Features.DbBackup;

public interface IPostgresqlBackup : IPotScopedDependency
{
    Task ExecuteAsync(string backupPath, CancellationToken cancellationToken);
}
