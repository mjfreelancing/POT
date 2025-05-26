using Pot.Data.Entities;
using Pot.Data.Repositories.Incomes;

namespace Pot.AspNetCore.Features.Incomes.Update.Services.EntityChecks;

internal sealed class InputState
{
    public required Request Request { get; init; }
    public required AccountEntity IncomeAccount { get; set; }
    public required IncomeEntity IncomeToUpdate { get; init; }
    public required IIncomeRepository IncomeRepository { get; init; }
    public required ILogger Logger { get; init; }
}
