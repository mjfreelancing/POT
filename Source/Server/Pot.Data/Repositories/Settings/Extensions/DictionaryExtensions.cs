using AllOverIt.Extensions;

namespace Pot.Data.Repositories.Settings.Extensions;

internal static class DictionaryExtensions
{
    public static bool GetBool(this Dictionary<string, string?> keyedSettings, string keyName, bool defaultValue)
    {
        return keyedSettings.TryGetValue(keyName, out var stringValue) && bool.TryParse(stringValue, out var value)
            ? value
            : defaultValue;
    }

    public static string GetString(this Dictionary<string, string?> keyedSettings, string keyName, string defaultValue)
    {
        return keyedSettings.TryGetValue(keyName, out var value) && value.IsNotNullOrEmpty()
            ? value
            : defaultValue;
    }

    public static int GetInt(this Dictionary<string, string?> keyedSettings, string keyName, int defaultValue)
    {
        return keyedSettings.TryGetValue(keyName, out var stringValue) && int.TryParse(stringValue, out var value)
            ? value
            : defaultValue;
    }
}