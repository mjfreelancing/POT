namespace Pot.Data.Repositories.Settings.Helpers;

internal static class ValueConversionHelper
{
    public static bool GetBool(Dictionary<string, string?> keyedSettings, string keyName, bool defaultValue)
    {
        return keyedSettings.TryGetValue(keyName, out var stringValue) && bool.TryParse(stringValue, out var value)
            ? value
            : defaultValue;
    }

    public static string? GetString(Dictionary<string, string?> keyedSettings, string keyName, string? defaultValue)
    {
        return keyedSettings.TryGetValue(keyName, out var value)
            ? value
            : defaultValue;
    }

    public static int GetInt(Dictionary<string, string?> keyedSettings, string keyName, int defaultValue)
    {
        return keyedSettings.TryGetValue(keyName, out var stringValue) && int.TryParse(stringValue, out var value)
            ? value
            : defaultValue;
    }
}