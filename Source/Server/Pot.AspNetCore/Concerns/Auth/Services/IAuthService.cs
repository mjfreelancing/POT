using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.Auth.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Concerns.Auth.Services;

public interface IAuthService : IPotScopedDependency
{
    Task<EnrichedResult<AuthTokens?>> LoginAsync(string username, string password, CancellationToken cancellationToken);
    Task<EnrichedResult<bool>> LogoutAsync(Guid userId, CancellationToken cancellationToken);
    Task<EnrichedResult<AuthTokens?>> RefreshAsync(string? accessToken, string refreshToken, CancellationToken cancellationToken);
    Task<EnrichedResult<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
}
