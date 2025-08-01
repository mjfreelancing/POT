using Pot.Shared;

namespace Pot.AspNetCore.Features.Incomes.Update;

internal sealed class RequestValidationContext
{
    public required Frequency Frequency { get; init; }
}