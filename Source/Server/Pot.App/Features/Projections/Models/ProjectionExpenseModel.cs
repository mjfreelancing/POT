namespace Pot.App.Features.Projections.Models;

/// <summary>
/// An expense falling due on a forecast day.
/// </summary>
/// <remarks>
/// Only expenses that are not excluded from calculations and have an amount greater than zero can fall due, and the
/// amount is the value of the single occurrence, not any accrual of it.
/// </remarks>
public sealed class ProjectionExpenseModel
{
    /// <summary>The expense identity, so the item can be attributed back to its row.</summary>
    public required Guid RowId { get; init; }

    /// <summary>The expense description.</summary>
    public required string Description { get; init; }

    /// <summary>The amount of the occurrence falling due.</summary>
    public required double Amount { get; init; }
}
