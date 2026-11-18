namespace Pot.App.Concerns.Money;

/// <summary>
/// Rounds monetary values to the two decimal places the product publishes.
/// </summary>
/// <remarks>
/// The conversion to <see cref="decimal"/> is deliberate rather than incidental. Most two-decimal amounts have no
/// exact binary representation, so a value such as 1.005 is held just below the midpoint and rounding the
/// <see cref="double"/> answers 1.00 where the decimal a person reads rounds to 1.01. Rounding the decimal
/// representation keeps the published figure and the printed figure the same.
///
/// A value that rounds to nothing is published as a positive zero. The sign survives the conversion, so rounding a
/// tiny negative value yields a negative zero, and a payload carrying -0 is formatted as -0.00 by a client.
/// </remarks>
internal static class MonetaryRounding
{
    /// <summary>The number of decimal places a monetary value is published to.</summary>
    private const int Scale = 2;

    /// <summary>
    /// Rounds a monetary value to two decimal places, away from zero.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value.</returns>
    internal static double Round(double value)
    {
        // A non-finite value has no two-decimal form and converting it would throw, turning malformed input into a
        // failure inside a calculation that otherwise tolerates it. Returning it unchanged keeps the problem with
        // the value that produced it.
        if (!double.IsFinite(value))
        {
            return value;
        }

        var rounded = Math.Round((decimal)value, Scale, MidpointRounding.AwayFromZero);

        return rounded == 0.0m ? 0.0d : (double)rounded;
    }
}
