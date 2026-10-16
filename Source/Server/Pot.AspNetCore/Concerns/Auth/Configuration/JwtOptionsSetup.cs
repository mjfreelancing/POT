using AllOverIt.Extensions;
using Microsoft.Extensions.Options;
using Pot.AspNetCore.Concerns.Auth.Models;

namespace Pot.AspNetCore.Concerns.Auth.Configuration;

/// <summary>Configures and validates JWT token settings.</summary>
public class JwtOptionsSetup : IConfigureOptions<JwtOptions>, IValidateOptions<JwtOptions>
{
    private const string SectionName = "Jwt";

    private readonly IConfiguration _configuration;

    /// <summary>Creates a configuration binder and validator for JWT settings.</summary>
    /// <param name="configuration">The application configuration source.</param>
    public JwtOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Applies configured values to the supplied options instance.</summary>
    /// <param name="options">The options instance to configure or validate.</param>
    public void Configure(JwtOptions options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }

    /// <summary>Validates the supplied value and returns the validation result.</summary>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="options">The options instance to configure or validate.</param>
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (options.Issuer.IsNullOrEmpty())
        {
            return InvalidOption(nameof(JwtOptions.Issuer));
        }

        if (options.Audience.IsNullOrEmpty())
        {
            return InvalidOption(nameof(JwtOptions.Audience));
        }

        if (options.SecretKey.IsNullOrEmpty())
        {
            return InvalidOption(nameof(JwtOptions.SecretKey));
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult InvalidOption(string optionName)
    {
        return ValidateOptionsResult.Fail($"JWT option '{optionName}' must be provided.");
    }
}
