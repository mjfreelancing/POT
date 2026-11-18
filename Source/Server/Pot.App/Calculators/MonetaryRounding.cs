namespace Pot.App.Calculators;

/// <summary>
/// Rounds monetary values to the two decimal places the product publishes.
/// </summary>
/// <remarks>
/// The conversion to <see cref="decimal"/> is deliberate rather than incidental. Most two-decimal amounts have no
/// exact binary representation, so a value such as 1.005 is held just below the midpoint and rounding the
/// <see cref="double"/> answers 1.00 where the decimal a person reads rounds to 1.01. Rounding the decimal
/// representation keeps the published figure and the printed figure the same. A <see cref="decimal"/> also has no
/// negative zero, so a value that rounds to nothing is published as zero rather than as a negative zero, which is
/// what a client formatting money renders as -0.00.
/// </remarks>
internal static class MonetaryRounding
{
    /// <summary>The number of decimal places a monetary value is published to.</summary>
    internal const int Scale = 2;

    /// <summary>
    /// Rounds a monetary value to <see cref="Scale"/> decimal places, away from zero.
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

        return (double)Math.Round((decimal)value, Scale, MidpointRounding.AwayFromZero);
    }
}
