using AllOverIt.Assertion;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pot.App.Concerns.Time;
using Pot.AspNetCore.Concerns.Auth.Models;
using Pot.Data.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pot.AspNetCore.Concerns.Auth;

internal sealed class JwtService : IJwtService
{
    private const int AccessTokenExpiryMins = 60;   // Recommended is 15-60 minutes

    private readonly JwtOptions _options;
    private readonly ITimeProvider _timeProvider;

    public JwtService(IOptions<JwtOptions> options, ITimeProvider timeProvider)
    {
        _options = options.Value.WhenNotNull();
        _timeProvider = timeProvider.WhenNotNull();
    }

    public string CreateAccessToken(UserEntity user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.RowId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = _timeProvider.GetUtcDateTimeNow().AddMinutes(AccessTokenExpiryMins),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
    {
        var secretKey = Encoding.UTF8.GetBytes(_options.SecretKey);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler
        {
            // Prevent JwtRegisteredClaimNames.Sub being mapped to ClaimTypes.NameIdentifier
            MapInboundClaims = false
        };

        var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid access token");
        }

        return principal;
    }
}
