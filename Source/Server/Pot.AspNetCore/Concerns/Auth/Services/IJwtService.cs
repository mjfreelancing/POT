using Pot.Data.Entities;
using Pot.Shared.DependencyInjection;
using System.Security.Claims;

namespace Pot.AspNetCore.Concerns.Auth.Services;

/// <summary>Defines operations for creating access tokens and reading their claims.</summary>
public interface IJwtService : IPotSingletonDependency
{
    /// <summary>Creates a signed access token for a user.</summary>
    /// <param name="user">The user whose claims are included in the token.</param>
    string CreateAccessToken(UserEntity user);
    /// <summary>Reads the claims principal from an expired access token.</summary>
    /// <param name="accessToken">The access token to inspect.</param>
    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
}
