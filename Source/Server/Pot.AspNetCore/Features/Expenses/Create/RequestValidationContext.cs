using Pot.Shared.Enumerations;

namespace Pot.AspNetCore.Features.Expenses.Create;

internal sealed class RequestValidationContext
{
    public required DateOnly NextDue { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required Frequency Frequency { get; init; }
}