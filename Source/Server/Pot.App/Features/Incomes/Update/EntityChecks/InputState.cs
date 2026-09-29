using Pot.App.Features.Incomes.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Incomes.Update.EntityChecks;

/// <summary>
/// Carries the values being validated through the pre-update check pipeline.
/// </summary>
internal sealed class InputState
{
    /// <summary>
    /// Gets the requested income update.
    /// </summary>
    public required Input Input { get; init; }

    /// <summary>
    /// Gets or sets the account the income belongs to.
    /// </summary>
    public required AccountEntity IncomeAccount { get; set; }

    /// <summary>
    /// Gets the persisted income being updated.
    /// </summary>
    public required IncomeEntity IncomeToUpdate { get; init; }
}
