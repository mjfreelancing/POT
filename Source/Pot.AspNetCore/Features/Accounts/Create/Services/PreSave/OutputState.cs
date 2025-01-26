using AllOverIt.Patterns.Result;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Create.Services.PreSave;

public sealed class OutputState
{
    public required EnrichedResult<AccountEntity> FailResult { get; init; }
}

