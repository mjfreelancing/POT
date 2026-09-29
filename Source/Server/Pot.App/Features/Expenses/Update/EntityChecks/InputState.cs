using Pot.App.Features.Expenses.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Expenses.Update.EntityChecks;

/// <summary>
/// Carries the values being validated through the pre-update check pipeline.
/// </summary>
internal sealed class InputState
{
    /// <summary>
    /// Gets the requested expense update.
    /// </summary>
    public required Input Input { get; init; }

    /// <summary>
    /// Gets or sets the account the expense belongs to.
    /// </summary>
    public required AccountEntity ExpenseAccount { get; set; }

    /// <summary>
    /// Gets the persisted expense being updated.
    /// </summary>
    public required ExpenseEntity ExpenseToUpdate { get; init; }
}
