namespace Pot.App.Features.Projections.Models;

/// <summary>
/// An income falling due on a forecast day.
/// </summary>
/// <remarks>
/// Only income that is not excluded from calculations and has an amount greater than zero can fall due, and the
/// amount is the value of the single occurrence.
/// </remarks>
public sealed class ProjectionIncomeModel
{
    /// <summary>The income identity, so the item can be attributed back to its row.</summary>
    public required Guid RowId { get; init; }

    /// <summary>The income description.</summary>
    public required string Description { get; init; }

    /// <summary>The amount of the occurrence falling due.</summary>
    public required double Amount { get; init; }
}
