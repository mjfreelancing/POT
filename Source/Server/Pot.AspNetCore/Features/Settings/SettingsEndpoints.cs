namespace Pot.AspNetCore.Features.Settings;

internal static class SettingsEndpoints
{
    public const string Group = $"{ApiEndpoints.ApiBase}/settings";
    public const string Tag = "Settings";

    public const string GetAll = "";
    public const string Update = "{category}/{key}";
}
