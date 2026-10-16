namespace Pot.AspNetCore.Concerns.Cors.Configuration;

/// <summary>Provides the configured origins allowed by the cross-origin resource sharing policy.</summary>
public sealed class CorsConfiguration
{
    /// <summary>Gets or initializes the configured cross-origin resource sharing origins.</summary>
    public required string AllowedOrigins { get; init; }

    /// <summary>Gets the configured origins allowed by the cross-origin resource sharing policy.</summary>
    public IReadOnlyCollection<string> GetAllowedOrigins()
    {
        return [.. AllowedOrigins
            .Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }
}
