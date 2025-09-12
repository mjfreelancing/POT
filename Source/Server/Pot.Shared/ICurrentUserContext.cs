using Pot.Shared.DependencyInjection;

namespace Pot.Shared;

public interface ICurrentUserContext : IPotScopedDependency
{
    void SetUser(Guid userRowId);
}
