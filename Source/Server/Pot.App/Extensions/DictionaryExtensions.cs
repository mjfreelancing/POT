namespace Pot.App.Extensions;

internal static class DictionaryExtensions
{
    extension(Dictionary<string, string> keyedSettings)
    {
        public bool GetBool(string keyName, bool defaultValue)
        {
            return keyedSettings.TryGetValue(keyName, out var stringValue)
                ? stringValue.AsBoolean(defaultValue)
                : defaultValue;
        }

        public string GetString(string keyName, string defaultValue)
        {
            return keyedSettings.TryGetValue(keyName, out var stringValue)
                ? stringValue.AsNonEmptyString(defaultValue)        // Considers null values and empty strings as not set - default value will be used
                : defaultValue;
        }

        public int GetInt(string keyName, int defaultValue)
        {
            return keyedSettings.TryGetValue(keyName, out var stringValue)
                ? stringValue.AsInt(defaultValue)
                : defaultValue;
        }
    }
}
