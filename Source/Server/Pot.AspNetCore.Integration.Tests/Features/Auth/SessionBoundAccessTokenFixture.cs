using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Pot.AspNetCore.Concerns.Auth;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.Data;
using Pot.Data.Entities;
using Shouldly;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Pot.AspNetCore.Integration.Tests.Features.Auth;

/// <summary>
/// Pins the session binding of access tokens: a token is only accepted while the login session it was issued for is
/// still active, which is what stops a token outliving the logout that revoked its session.
/// </summary>
public class SessionBoundAccessTokenFixture : IntegrationAuthFixtureBase
{
    [Fact]
    public async Task Should_Accept_A_Token_That_Names_Its_Issued_Session()
    {
        var user = await CreateEnabledUserAsync("session", "Session User");
        var loginResult = await LoginAsync(user);

        var issuedSessionRowId = ReadSessionRowId(loginResult.AccessToken!);
        var reissuedToken = CreateAccessTokenLike(loginResult.AccessToken!, issuedSessionRowId);

        var currentUserStatus = await GetCurrentUserAsync(reissuedToken);

        currentUserStatus.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Reject_A_Token_Without_A_Session_Claim()
    {
        var user = await CreateEnabledUserAsync("session", "Session User");
        var loginResult = await LoginAsync(user);

        var tokenWithoutSession = CreateAccessTokenLike(loginResult.AccessToken!, sessionRowId: null);

        var currentUserStatus = await GetCurrentUserAsync(tokenWithoutSession);

        currentUserStatus.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Reject_A_Token_Whose_Named_Session_Does_Not_Exist()
    {
        var user = await CreateEnabledUserAsync("session", "Session User");
        var loginResult = await LoginAsync(user);

        var unknownSessionRowId = Guid.NewGuid();
        var tokenWithUnknownSession = CreateAccessTokenLike(loginResult.AccessToken!, unknownSessionRowId);

        var currentUserStatus = await GetCurrentUserAsync(tokenWithUnknownSession);

        currentUserStatus.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Reject_An_Access_Token_When_Its_Session_Has_Expired()
    {
        var user = await CreateEnabledUserAsync("session", "Session User");
        var loginResult = await LoginAsync(user);

        var issuedSessionRowId = ReadSessionRowId(loginResult.AccessToken!);

        await ExpireSessionAsync(issuedSessionRowId);

        var currentUserStatus = await GetCurrentUserAsync(loginResult.AccessToken!);

        currentUserStatus.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Reject_A_Token_That_Names_Another_Users_Session()
    {
        var owner = await CreateEnabledUserAsync("session", "Session User");
        var otherUser = await CreateEnabledUserAsync("cross-user", "Cross User");

        var ownerLogin = await LoginAsync(owner);
        var otherUserLogin = await LoginAsync(otherUser);

        var otherUsersSessionRowId = ReadSessionRowId(otherUserLogin.AccessToken!);
        var tokenNamingAnotherUsersSession = CreateAccessTokenLike(ownerLogin.AccessToken!, otherUsersSessionRowId);

        var currentUserStatus = await GetCurrentUserAsync(tokenNamingAnotherUsersSession);

        currentUserStatus.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Reject_A_Token_Whose_Session_Claim_Is_Not_A_Key()
    {
        var user = await CreateEnabledUserAsync("session", "Session User");
        var loginResult = await LoginAsync(user);

        var tokenWithMalformedSession = CreateAccessTokenLike(loginResult.AccessToken!, sessionIdClaim: "not-a-session-row-id");

        var currentUserStatus = await GetCurrentUserAsync(tokenWithMalformedSession);

        currentUserStatus.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private async Task ExpireSessionAsync(Guid sessionRowId)
    {
        using var scope = CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PotDbContext>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var authSession = await dbContext.Set<AuthSessionEntity>()
            .AsTracking()
            .SingleAsync(session => session.RowId == sessionRowId, cancellationToken);

        authSession.ExpiresUtc = DateTime.UtcNow.AddMinutes(-1);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Guid ReadSessionRowId(string accessToken)
    {
        var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var sessionIdClaim = token.Claims.SingleOrDefault(claim => claim.Type == PotClaimTypes.SessionId);

        sessionIdClaim.ShouldNotBeNull();

        return Guid.Parse(sessionIdClaim.Value);
    }

    private static string CreateAccessTokenLike(string accessToken, Guid? sessionRowId)
    {
        return CreateAccessTokenLike(accessToken, sessionRowId?.ToString());
    }

    /// <remarks>
    /// Signed with the same issuer, audience and key as the host, and carrying the claims the host validates, so the
    /// only thing under test is the session claim. The session claim is left out of the token when
    /// <paramref name="sessionIdClaim" /> is null.
    /// </remarks>
    private static string CreateAccessTokenLike(string accessToken, string? sessionIdClaim)
    {
        var referenceToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, GetClaimValue(referenceToken, JwtRegisteredClaimNames.Jti)),
            new(JwtRegisteredClaimNames.Sub, GetClaimValue(referenceToken, JwtRegisteredClaimNames.Sub)),
            new(PotClaimTypes.TokenVersion, GetClaimValue(referenceToken, PotClaimTypes.TokenVersion))
        };

        if (sessionIdClaim is not null)
        {
            claims.Add(new Claim(PotClaimTypes.SessionId, sessionIdClaim));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(5),
            Issuer = ApiWebApplicationFactory.TestJwtIssuer,
            Audience = ApiWebApplicationFactory.TestJwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiWebApplicationFactory.TestJwtSecretKey)),
                SecurityAlgorithms.HmacSha512)
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }

    private static string GetClaimValue(JwtSecurityToken token, string claimType)
    {
        return token.Claims.Single(claim => claim.Type == claimType).Value;
    }
}
