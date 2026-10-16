using Pot.App.Features.Me.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Features.Me.Services;

/// <summary>Provides HTTP-facing operations for retrieving current-user information.</summary>
public interface IHttpUserService : IPotScopedDependency
{
    /// <summary>Retrieves the current user's information from the HTTP request context.</summary>
    /// <param name="httpContext">The current HTTP request context.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task<Output?> GetMeInfoAsync(HttpContext httpContext, CancellationToken cancellationToken);
}
