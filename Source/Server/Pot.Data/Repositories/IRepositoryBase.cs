using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.Data.Repositories;

public interface IRepositoryBase : IPotScopedDependency
{
    // On IRepositoryBase just in case a read entity needs to be attached to another processed by IPersistableRepository
    IDisposable WithTracking();

    EntityEntry GetEntry<TEntity>(TEntity entity) where TEntity : EntityBase;

    // Deliberately no Set<TEntity>() here: a queryable on the interface would let any caller write a lookup with
    // no site predicate, which is what the site-scoped repository methods exist to prevent. It stays on
    // RepositoryBase for repository implementations to use.
}
