using Pot.Shared.DependencyInjection;

namespace Pot.Shared;

public interface ICurrentUserContext : IPotScopedDependency
{
    Guid UserRowId { get; }

    void SetUserRowId(Guid userRowId);
}
