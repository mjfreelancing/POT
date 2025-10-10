using AllOverIt.Extensions;
using Microsoft.Extensions.Options;
using Pot.AspNetCore.Concerns.Auth.Models;

namespace Pot.AspNetCore.Concerns.Auth.Configuration;

public class JwtOptionsSetup : IConfigureOptions<JwtOptions>, IValidateOptions<JwtOptions>
{
    private const string SectionName = "Jwt";

    private readonly IConfiguration _configuration;

    public JwtOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(JwtOptions options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }

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
