using Pot.Shared;

namespace Pot.AspNetCore.Features.Expenses.Update;

internal sealed class RequestValidationContext
{
    public required Frequency Frequency { get; init; }
}
