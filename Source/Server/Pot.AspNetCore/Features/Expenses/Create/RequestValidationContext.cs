using Pot.Shared;

namespace Pot.AspNetCore.Features.Expenses.Create;

internal sealed class RequestValidationContext
{
    public required Frequency Frequency { get; init; }
}