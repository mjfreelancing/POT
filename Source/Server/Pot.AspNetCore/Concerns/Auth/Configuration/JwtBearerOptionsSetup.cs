using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pot.AspNetCore.Concerns.Auth.Models;
using System.Text;

namespace Pot.AspNetCore.Concerns.Auth.Configuration;

/// <summary>Applies JWT settings to bearer authentication options.</summary>
public class JwtBearerOptionsSetup : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOptions;

    /// <summary>Creates a post-configuration step using the supplied JWT settings.</summary>
    /// <param name="jwtOptions">The configured JWT settings.</param>
    public JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    /// <summary>Applies JWT settings after the bearer options have been configured.</summary>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="options">The options instance to configure or validate.</param>
    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        // These are applied as if there were part of the AddAuthentication setup:
        // .AddJwtBearer(options => { options.TokenValidationParameters = new TokenValidationParameters { ValidateLifetime = .... } })

        options.TokenValidationParameters.ValidateLifetime = true;

        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidIssuer = _jwtOptions.Issuer;

        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidAudience = _jwtOptions.Audience;

        options.TokenValidationParameters.ValidateIssuerSigningKey = true;
        options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
    }
}
