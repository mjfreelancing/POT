using Pot.Data.Entities;

namespace Pot.App.Features.Incomes.Create.EntityChecks;

/// <summary>
/// Carries the income being validated through the pre-create check pipeline.
/// </summary>
internal sealed class InputState
{
    /// <summary>
    /// Gets the income pending creation.
    /// </summary>
    public required IncomeEntity IncomeToCreate { get; init; }
}
