using AllOverIt.Extensions;

namespace Pot.App.Extensions;

internal static class DictionaryExtensions
{
    extension(Dictionary<string, string?> keyedSettings)
    {
        public bool GetBool(string keyName, bool defaultValue)
        {
            return keyedSettings.TryGetValue(keyName, out var stringValue) && bool.TryParse(stringValue, out var value)
                ? value
                : defaultValue;
        }

        public string GetString(string keyName, string defaultValue)
        {
            return keyedSettings.TryGetValue(keyName, out var value) && value.IsNotNullOrEmpty()
                ? value
                : defaultValue;
        }

        public int GetInt(string keyName, int defaultValue)
        {
            return keyedSettings.TryGetValue(keyName, out var stringValue) && int.TryParse(stringValue, out var value)
                ? value
                : defaultValue;
        }
    }
}