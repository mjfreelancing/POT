using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Pot.AspNetCore.Concerns.RateLimiting.Configuration;

/// <summary>
/// Binds the <c>RateLimiting</c> configuration section onto <see cref="RateLimitingConfiguration"/>.
/// Registered via <c>ConfigureOptions&lt;RateLimitingConfigurationSetup&gt;()</c> so that
/// <see cref="RateLimiterOptionsSetup"/> can receive the resolved configuration through DI.
/// </summary>
internal sealed class RateLimitingConfigurationSetup : IConfigureOptions<RateLimitingConfiguration>
{
    private const string SectionName = "RateLimiting";

    private readonly IConfiguration _configuration;

    public RateLimitingConfigurationSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(RateLimitingConfiguration options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }
}
