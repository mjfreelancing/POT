namespace Pot.AspNetCore.Extensions;

internal static class ConfigurationExtensions
{
    public static bool IsProduction(this IConfiguration configuration)
    {
        var environment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");
        return string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase);
    }
}