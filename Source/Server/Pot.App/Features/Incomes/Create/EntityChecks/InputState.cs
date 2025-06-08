using Pot.Data.Entities;

namespace Pot.App.Features.Incomes.Create.EntityChecks;

internal sealed class InputState
{
    public required IncomeEntity IncomeToCreate { get; init; }
}
