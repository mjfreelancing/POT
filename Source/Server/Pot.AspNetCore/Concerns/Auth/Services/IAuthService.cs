using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.Auth.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.AspNetCore.Concerns.Auth.Services;

/// <summary>Defines authentication operations for signing in, signing out, refreshing tokens and changing passwords.</summary>
public interface IAuthService : IPotScopedDependency
{
    /// <summary>Authenticates a user and establishes an authentication session.</summary>
    /// <param name="username">The username supplied for authentication.</param>
    /// <param name="password">The password supplied for authentication.</param>
    /// <param name="context">The user-agent and client address associated with this sign-in request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The authentication result, containing tokens when sign-in succeeds.</returns>
    Task<EnrichedResult<AuthTokens?>> LoginAsync(string username, string password, LoginRequestContext context, CancellationToken cancellationToken);
    /// <summary>Ends the user's current authentication session.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="refreshToken">The refresh token associated with the session.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The result indicating whether the current session was ended.</returns>
    Task<EnrichedResult<bool>> LogoutAsync(Guid userId, string? refreshToken, CancellationToken cancellationToken);
    /// <summary>Issues refreshed authentication tokens for the supplied session credentials.</summary>
    /// <param name="accessToken">The current access token, when one is available.</param>
    /// <param name="refreshToken">The refresh token associated with the session.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The authentication result, containing refreshed tokens when the credentials are valid.</returns>
    Task<EnrichedResult<AuthTokens?>> RefreshAsync(string? accessToken, string refreshToken, CancellationToken cancellationToken);
    /// <summary>Changes a user's password after validating the current password.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="currentPassword">The user's current password.</param>
    /// <param name="newPassword">The replacement password.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The result indicating whether the password was changed.</returns>
    Task<EnrichedResult<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
}
