using Pot.Shared;

namespace Pot.App.Features.Expenses.GetAll.Models;

public sealed class Output
{
    public sealed class AccountModel
    {
        public required Guid RowId { get; init; }
        public required string Description { get; init; }
    }

    public required Guid RowId { get; init; }
    public required long Etag { get; init; }
    public required string Description { get; init; }
    public required DateOnly AccrualStart { get; init; }    // Can adjust this date to start accruing sooner/later (paid previous earlier or want to delay)
    public required DateOnly NextDue { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required Frequency Frequency { get; init; }
    public required int FrequencyCount { get; init; }
    public required double Amount { get; init; }
    public required double Accrued { get; init; }
    public string? Note { get; init; }
    public required AccountModel Account { get; init; }
}
