using AllOverIt.Extensions;

namespace Pot.App.Extensions;

internal static class StringExtensions
{
    extension(string? stringValue)
    {
        public bool TryAsBoolean(out bool value)
        {
            return bool.TryParse(stringValue, out value);
        }

        public bool AsBoolean(bool defaultValue)
        {
            return stringValue.TryAsBoolean(out var value)
                ? value
                : defaultValue;
        }

        public bool TryAsInt(out int value)
        {
            return int.TryParse(stringValue, out value);
        }

        public int AsInt(int defaultValue)
        {
            return stringValue.TryAsInt(out var value)
                ? value
                : defaultValue;
        }

        public string AsNonEmptyString(string defaultValue)
        {
            return stringValue.IsNotNullOrEmpty()
                ? stringValue
                : defaultValue;
        }
    }
}