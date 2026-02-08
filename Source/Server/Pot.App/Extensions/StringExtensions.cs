using AllOverIt.Extensions;

namespace Pot.App.Extensions;

internal static class StringExtensions
{
    extension(string? stringValue)
    {
        public bool AsBoolean(bool defaultValue)
        {
            return bool.TryParse(stringValue, out var value)
                ? value
                : defaultValue;
        }

        public int AsInt(int defaultValue)
        {
            return int.TryParse(stringValue, out var value)
                ? value
                : defaultValue;
        }

        public string AsString(string defaultValue)
        {
            return stringValue.IsNotNullOrEmpty()
                ? stringValue
                : defaultValue;
        }
    }
}