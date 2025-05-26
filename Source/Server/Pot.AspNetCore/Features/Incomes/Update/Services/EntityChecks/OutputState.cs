using AllOverIt.Patterns.Result;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Incomes.Update.Services.EntityChecks;

public sealed class OutputState
{
    public required EnrichedResult<IncomeEntity> FailResult { get; init; }
}

