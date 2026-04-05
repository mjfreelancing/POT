namespace Pot.App.Features.Expenses.Update.Models;

public sealed class Output
{
    public required Guid RowId { get; init; }
    public required long Etag { get; init; }

    // Included because server-side business rules may canonicalize this value from the submitted input.
    public required DateOnly? AccrualStart { get; init; }
}
