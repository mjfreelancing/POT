using Pot.Shared;

namespace Pot.App.Features.Expenses.Get.Models;

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
    public required DateOnly AccrualStart { get; init; }
    public required DateOnly NextDue { get; init; }
    public DateOnly? EndDate { get; init; }
    public required Frequency Frequency { get; init; }  // Serialized via EnrichedEnumJsonConverter<Frequency>
    public required int FrequencyCount { get; init; }
    public required double Amount { get; init; }
    public required bool Recurring { get; init; }
    public required AccountModel Account { get; init; }
}
