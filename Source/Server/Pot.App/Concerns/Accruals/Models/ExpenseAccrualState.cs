using Pot.Shared.Enumerations;

namespace Pot.App.Concerns.Accruals.Models;

public sealed record ExpenseAccrualState
{
    public required int AccountId { get; init; }
    public required bool ExcludeFromCalcs { get; init; }
    public required DateOnly? AccrualStart { get; init; }
    public required DateOnly NextDue { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required AccrualPolicy AccrualPolicy { get; init; }
    public required Frequency Frequency { get; init; }
    public required int FrequencyCount { get; init; }
    public required double Amount { get; init; }
}