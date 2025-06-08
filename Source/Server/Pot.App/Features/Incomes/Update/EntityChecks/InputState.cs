using Pot.App.Features.Incomes.Update.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Incomes.Update.EntityChecks;

internal sealed class InputState
{
    public required Input Input { get; init; }
    public required AccountEntity IncomeAccount { get; set; }
    public required IncomeEntity IncomeToUpdate { get; init; }
}
