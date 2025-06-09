using Pot.Data.Entities;

namespace Pot.App.Features.Expenses.Create.EntityChecks;

internal sealed class InputState
{
    public required ExpenseEntity ExpenseToCreate { get; init; }
}
