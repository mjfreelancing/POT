using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pot.Data.Repositories.Users;
using System.IdentityModel.Tokens.Jwt;

namespace Pot.AspNetCore.Concerns.Auth.Configuration;

// Purpose: Configures JWT Bearer authentication event handlers to validate tokens against database state.
//
// This class addresses a critical security issue with stateless JWT tokens:
// - JWTs are self-contained and remain valid until expiration (15 minutes by default)
// - When a user logs out, the refresh token is cleared from the database
// - However, the access token (JWT) continues to work until it expires naturally
// - This creates a 15-minute window where revoked tokens remain valid
//
// Solution: Token Version Validation
// - Each user has a TokenVersion field in the database (integer, starts at 0)
// - When a JWT is created, the current TokenVersion is embedded as a claim
// - On every authenticated request, this class validates:
//   1. The token's embedded version matches the user's current database version
//   2. The user's account status is still 'Enabled'
// - When user logs out, TokenVersion is incremented, invalidating all existing tokens immediately
// - Same applies to password changes - incrementing TokenVersion forces re-login
//
// Performance Considerations:
// - Adds one database (indexed) query per authenticated request
// - Acceptable overhead for personal/small projects
// - For high-scale applications, consider adding Redis cache layer
//
// Why IConfigureNamedOptions<T>:
// - ASP.NET Core supports multiple named authentication schemes (e.g., "Bearer1", "Bearer2")
// - IConfigureNamedOptions allows configuring specific or default (null name) schemes
// - More robust than IConfigureOptions which only handles the default unnamed instance
// - Follows the same pattern as other JWT Bearer configuration classes
// - The 'name' parameter identifies which scheme is being configured
internal sealed class JwtBearerEventsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    // JwtBearerEventsSetup is a singleton, so we inject IServiceScopeFactory to create scopes for scoped services
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public JwtBearerEventsSetup(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                // Extract claims
                var userIdClaim = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                var tokenVersionClaim = context.Principal?.FindFirst(PotClaimTypes.TokenVersion)?.Value;

                if (userIdClaim is null || tokenVersionClaim is null ||
                    !Guid.TryParse(userIdClaim, out var userId) ||
                    !int.TryParse(tokenVersionClaim, out var tokenVersion))
                {
                    context.Fail("Invalid token claims");
                    return;
                }

                // Create scope to resolve scoped repository
                using var scope = _serviceScopeFactory.CreateScope();

                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                // Check current token version in database
                var currentVersion = await userRepository.Users
                    .Where(user => user.RowId == userId)
                    .Select(user => new { user.TokenVersion, user.Status })
                    .SingleOrDefaultAsync(context.HttpContext.RequestAborted);

                if (currentVersion == null)
                {
                    context.Fail("User not found");
                    return;
                }

                // Reject if token version doesn't match (user logged out)
                if (currentVersion.TokenVersion != tokenVersion)
                {
                    context.Fail("Token has been revoked");
                    return;
                }

                // Also check user is still enabled
                if (currentVersion.Status != Shared.Enumerations.UserStatus.Enabled)
                {
                    context.Fail("User account is not enabled");
                }
            }
        };
    }

    public void Configure(JwtBearerOptions options) => Configure(null, options);
}