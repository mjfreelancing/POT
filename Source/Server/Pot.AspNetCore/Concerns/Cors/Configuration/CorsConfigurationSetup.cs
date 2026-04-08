using AllOverIt.Extensions;
using Microsoft.Extensions.Options;

namespace Pot.AspNetCore.Concerns.Cors.Configuration;

public sealed class CorsConfigurationSetup : IConfigureOptions<CorsConfiguration>, IValidateOptions<CorsConfiguration>
{
    private const string SectionName = "Cors";

    private readonly IConfiguration _configuration;

    public CorsConfigurationSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(CorsConfiguration options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }

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
