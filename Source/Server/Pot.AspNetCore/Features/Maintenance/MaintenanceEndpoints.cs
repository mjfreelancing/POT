namespace Pot.AspNetCore.Features.Maintenance;

internal static class MaintenanceEndpoints
{
    public const string Group = $"{ApiEndpoints.ApiBase}/maintenance";
    public const string Tag = "Maintenance Api";

    public const string Export = "/export";
}