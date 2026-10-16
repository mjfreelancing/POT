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
        if (options.AllowedOrigins.IsNullOrEmpty() || options.GetAllowedOrigins().Count == 0)
        {
            return InvalidOption(nameof(CorsConfiguration.AllowedOrigins));
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult InvalidOption(string optionName)
    {
        return ValidateOptionsResult.Fail($"Cors option '{optionName}' must be provided.");
    }
}
