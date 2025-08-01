using Pot.Shared;

namespace Pot.AspNetCore.Features.Incomes.Create;

internal sealed class RequestValidationContext
{
    public required Frequency Frequency { get; init; }
}

