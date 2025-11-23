using Microsoft.Extensions.Options;

namespace Pot.AspNetCore.Concerns.Auth.Configuration;

public class AuthenticationOptionsSetup : IConfigureOptions<AuthenticationOptions>, IValidateOptions<AuthenticationOptions>
{
    private const string SectionName = "Authentication";

    private readonly IConfiguration _configuration;

    public AuthenticationOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(AuthenticationOptions options)
    {
        // Initialize Cookie with defaults first to prevent null reference
        options.Cookie = new AuthenticationOptions.CookieOptions();

        // Then bind configuration values (will override defaults if present)
        _configuration.GetSection(SectionName).Bind(options);

        // Ensure Cookie is initialized even if missing from config
        options.Cookie ??= new AuthenticationOptions.CookieOptions();
    }

    public ValidateOptionsResult Validate(string? name, AuthenticationOptions options)
    {
        // options.UserIds can be null/empty since PlatformAdminOptions will return []
        if (options.Cookie is null)
        {
            return InvalidOption(nameof(AuthenticationOptions.Cookie));
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult InvalidOption(string optionName)
    {
        return ValidateOptionsResult.Fail($"{SectionName} option '{optionName}' must be provided.");
    }
}
