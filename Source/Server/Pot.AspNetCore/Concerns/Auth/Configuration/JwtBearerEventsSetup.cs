using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Pot.App.Concerns.Time;
using Pot.Data.Repositories.Users;
using System.IdentityModel.Tokens.Jwt;

namespace Pot.AspNetCore.Concerns.Auth.Configuration;

// Purpose: Configures JWT Bearer authentication event handlers to validate tokens against database state.
//
// This class addresses a critical limitation of stateless JWT tokens:
// - JWTs are self-contained and remain valid until expiration (15 minutes by default)
// - Revoking a session row does not by itself stop a token that was already issued
//
// Solution: Token Version and Session Validation
// - Each user has a TokenVersion field in the database (integer, starts at 0)
// - When a JWT is created, the current TokenVersion is embedded as a claim
// - Each access token also carries the RowId of the AuthSession it was issued for (session_id)
// - On every authenticated request, this class validates:
//   1. The token's embedded version matches the user's current database version
//   2. The token's session still exists, is not revoked and has not expired
//   3. The user's account status is still 'Enabled'
// - A password change increments TokenVersion and revokes every session, forcing a re-login
// - Logging out on one device revokes that session, so only that device's access token is rejected
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
                var sessionIdClaim = context.Principal?.FindFirst(PotClaimTypes.SessionId)?.Value;

                if (userIdClaim is null || tokenVersionClaim is null || sessionIdClaim is null ||
                    !Guid.TryParse(userIdClaim, out var userId) ||
                    !int.TryParse(tokenVersionClaim, out var tokenVersion) ||
                    !Guid.TryParse(sessionIdClaim, out var sessionRowId))
                {
                    context.Fail("Invalid token claims");
                    return;
                }

                // Create scope to resolve scoped repository
                using var scope = _serviceScopeFactory.CreateScope();

                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var timeProvider = scope.ServiceProvider.GetRequiredService<ITimeProvider>();

                // Check current token version in database
                // Cross-tenant by design: the token is validated before any user context exists, so its
                // subject is the only thing known.
                var currentVersion = await userRepository.GetSecurityStateAsync(userId, sessionRowId, context.HttpContext.RequestAborted);

                if (currentVersion == null)
                {
                    context.Fail("User not found");
                    return;
                }

                // Reject if token version doesn't match (user logged out, or credentials were changed)
                if (currentVersion.TokenVersion != tokenVersion)
                {
                    context.Fail("Token has been revoked");
                    return;
                }

                // Reject when the session that issued the token is gone, revoked, or expired, so signing out
                // on one device stops that device's access token without affecting the user's other sessions.
                if (currentVersion.SessionExpiresUtc is null ||
                    currentVersion.SessionRevokedUtc is not null ||
                    currentVersion.SessionExpiresUtc <= timeProvider.GetUtcDateTimeNow())
                {
                    context.Fail("Session is no longer active");
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