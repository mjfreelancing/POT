using AllOverIt.Extensions;
using Microsoft.Extensions.Options;

namespace Pot.AspNetCore.Concerns.Auth.Configuration;

internal sealed class PlatformAdminOptionsSetup : IConfigureOptions<PlatformAdminOptions>, IValidateOptions<PlatformAdminOptions>
{
    private const string SectionName = "PlatformAdmin";

    private readonly IConfiguration _configuration;

    public PlatformAdminOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(PlatformAdminOptions options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }

    public ValidateOptionsResult Validate(string? name, PlatformAdminOptions options)
    {
        // options.UserIds can be null/empty since PlatformAdminOptions will return []
        if (options.UserIds.IsNotNullOrEmpty())
        {
            // Check for any string values that are not valid GUIDs.
            if (options.UserIds
                   .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                   .Any(id => !Guid.TryParse(id, out _)))
            {
                return InvalidOption(nameof(PlatformAdminOptions.UserIds));
            }
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult InvalidOption(string optionName)
    {
        return ValidateOptionsResult.Fail($"Platform Admin option '{optionName}' must be provided.");
    }
}
