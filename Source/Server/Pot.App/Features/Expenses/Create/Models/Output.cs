namespace Pot.App.Features.Expenses.Create.Models;

public sealed class Output
{
    public required Guid RowId { get; init; }
    public required long Etag { get; init; }

    // Included because server-side business rules may canonicalize this value from the submitted input.
    // Not actually used by the client application, but it is appropriate to return since it can be different to the original input.
    public required DateOnly? AccrualStart { get; init; }
}
