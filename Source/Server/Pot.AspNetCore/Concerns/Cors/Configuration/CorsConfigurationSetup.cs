using AllOverIt.Extensions;
using Microsoft.Extensions.Options;

namespace Pot.AspNetCore.Concerns.Cors.Configuration;

/// <summary>Configures and validates cross-origin resource sharing settings.</summary>
public sealed class CorsConfigurationSetup : IConfigureOptions<CorsConfiguration>, IValidateOptions<CorsConfiguration>
{
    private const string SectionName = "Cors";

    private readonly IConfiguration _configuration;

    /// <summary>Creates a configuration binder and validator for cross-origin resource sharing settings.</summary>
    /// <param name="configuration">The application configuration source.</param>
    public CorsConfigurationSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Applies configured values to the supplied options instance.</summary>
    /// <param name="options">The options instance to configure or validate.</param>
    public void Configure(CorsConfiguration options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }

    /// <summary>Validates the supplied value and returns the validation result.</summary>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="options">The options instance to configure or validate.</param>
    public ValidateOptionsResult Validate(string? name, CorsConfiguration options)
    {
        if (options.AllowedOrigins.IsNullOrEmpty())
        {
            return InvalidOption(nameof(CorsConfiguration.AllowedOrigins));
        }

        var allowedOrigins = options.GetAllowedOrigins();

        if (allowedOrigins.Count == 0)
        {
            return InvalidOption(nameof(CorsConfiguration.AllowedOrigins));
        }

        foreach (var origin in allowedOrigins)
        {
            if (!IsValidOrigin(origin))
            {
                return InvalidOrigin(origin);
            }
        }

        return ValidateOptionsResult.Success;
    }

    private static bool IsValidOrigin(string origin)
    {
        // '*' is rejected explicitly: combined with AllowCredentials() it makes the policy fail when it
        // is first built, which is a request-time failure rather than a startup failure.
        if (origin == "*")
        {
            return false;
        }

        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            !string.IsNullOrEmpty(uri.UserInfo))
        {
            return false;
        }

        // A browser sends Origin as scheme://host[:port] with no path, query or fragment. Comparing the
        // whole value to the authority rejects a trailing slash or any path, which never matches the
        // browser's header and would make CORS silently fail for every request.
        return string.Equals(origin, uri.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase);
    }

    private static ValidateOptionsResult InvalidOption(string optionName)
    {
        return ValidateOptionsResult.Fail($"Cors option '{optionName}' must be provided.");
    }

    private static ValidateOptionsResult InvalidOrigin(string origin)
    {
        return ValidateOptionsResult.Fail(
            $"Cors option 'AllowedOrigins' contains an invalid origin '{origin}'. " +
            "Each entry must be an absolute http or https origin with no path, query, fragment or trailing slash.");
    }
}
