using Pot.Maui.Domain.Models;
using System.Globalization;

namespace Pot.Domain.Converters
{
    public sealed class ExpenseFrequencyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }

            return Enum.GetName(value.GetType(), value);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string stringValue)
            {
                return null;
            }

            return Enum.Parse(typeof(ExpenseFrequency), stringValue);
        }
    }
}
