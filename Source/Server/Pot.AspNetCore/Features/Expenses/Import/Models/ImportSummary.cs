namespace Pot.AspNetCore.Features.Expenses.Import.Models;

public sealed class ImportSummary
{
    public int Imported { get; init; }
    public int Updated { get; init; }
    public int Total { get; init; }
}
