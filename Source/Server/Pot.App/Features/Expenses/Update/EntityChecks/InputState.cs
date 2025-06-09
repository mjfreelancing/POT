using Pot.App.Features.Expenses.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Expenses.Update.EntityChecks;

internal sealed class InputState
{
    public required Input Input { get; init; }
    public required AccountEntity ExpenseAccount { get; set; }
    public required ExpenseEntity ExpenseToUpdate { get; init; }
}
