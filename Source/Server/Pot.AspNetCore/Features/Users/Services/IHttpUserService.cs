using Pot.App.Features.Auth.Me.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Features.Users.Services;

public interface IHttpUserService : IPotScopedDependency
{
    Task<UserInfo?> GetUserInfoAsync(HttpContext httpContext, CancellationToken cancellationToken);
}
