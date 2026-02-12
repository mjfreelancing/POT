using Pot.App.Errors;
using Pot.App.Features.Sites.Update.Models;
using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Sites.Update.EntityChecks;

public interface IPreUpdateChecker : IPotScopedDependency
{
    Task<ApiDetailError?> CanSaveAsync(Input input, SiteEntity siteToUpdate, CancellationToken cancellationToken);
}
