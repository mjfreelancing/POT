namespace Pot.AspNetCore.Concerns.Cors.Configuration;

public sealed class CorsConfiguration
{
    public required string AllowedOrigins { get; init; }

    public IReadOnlyCollection<string> GetAllowedOrigins()
    {
        return [.. AllowedOrigins
            .Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }
}
