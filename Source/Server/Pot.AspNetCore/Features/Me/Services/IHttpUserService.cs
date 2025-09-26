using Pot.App.Features.Auth.Me.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Features.Me.Services;

public interface IHttpUserService : IPotScopedDependency
{
    Task<Output?> GetMeInfoAsync(HttpContext httpContext, CancellationToken cancellationToken);
}
