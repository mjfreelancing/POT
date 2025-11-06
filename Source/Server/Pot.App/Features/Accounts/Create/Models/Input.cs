namespace Pot.App.Features.Accounts.Create.Models;

public sealed class Input
{
    // Provided when importing - required so income and expense items can be linked correctly
    public Guid? RowId { get; init; }

    public string Bsb { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public double Balance { get; init; }
    public double Reserved { get; init; }
}
