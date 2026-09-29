using Pot.Data.Entities;

namespace Pot.App.Features.Expenses.Create.EntityChecks;

/// <summary>
/// Carries the expense being validated through the pre-create check pipeline.
/// </summary>
internal sealed class InputState
{
    /// <summary>
    /// Gets the expense pending creation.
    /// </summary>
    public required ExpenseEntity ExpenseToCreate { get; init; }
}
