using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Incomes;

namespace Pot.AspNetCore.Features.Incomes.Create.Services.EntityChecks;

internal sealed class InputState
{
    public required Guid? AccountRowId { get; init; }
    public required IncomeEntity IncomeToCreate { get; init; }
    public required IAccountRepository AccountRepository { get; init; }
    public required IIncomeRepository IncomeRepository { get; init; }
    public required ILogger Logger { get; init; }
}
